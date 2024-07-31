using Core.Infrastructure.Service;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

namespace Assets.Code.Scripts.Infrastructure.Services.AudioService.Version
{
    public enum AudioGroupType
    {
        Master,
        Music,
        Sound
    }
    [CreateAssetMenu(fileName = "ClipEvent", menuName = "SO/Audio/ClipEvent")]
    public class ClipEvent : ScriptableObject
    {
        [Header("Audio Settings")]
        public AudioGroupType AudioGroup;
        public AssetReferenceAudioClip[] Clips;
        [Header("Properties")]
        public bool IsLoop;
    }
    [CreateAssetMenu(fileName = "AudioServiceConfig", menuName = "SO/Audio/AudioServiceConfig")]
    public class AudioServiceConfig : ScriptableObject
    {
        public TypeGroupKey[] TypeGroups;

        [Serializable]
        public class TypeGroupKey
        {
            public AudioGroupType Type;
            public AudioMixerGroup Group;
            public string VolumeProperty;
        }
    }
    public class SourceInstance : IDisposable
    {
        private AudioSource _source;
        private ClipEvent _clipReference;

        private int _currentClipIndex = 0;

        public bool IsPause { get; private set; }

        public SourceInstance(AudioSource source, ClipEvent clipReference)
        {
            _source = source;
            _clipReference = clipReference;
        }

        public async void Play()
        {
            _currentClipIndex = 0;
            AssetReferenceAudioClip clipReference = _clipReference.Clips[_currentClipIndex];
            _source.clip = await clipReference.GetOrLoad();
            AudioSourceEventModule module = _source.AddComponent<AudioSourceEventModule>();
            module.OnEndPlay += SetNextClip;
            _source.Play();
        }
        public void Stop() 
        {
            _source.Stop();
            if(_source.TryGetComponent(out AudioSourceEventModule module))
            {
                module.OnEndPlay -= SetNextClip;
                Component.Destroy(module);
            }
        }
        public void Pause(bool isPause)
        {
            IsPause = isPause;
            if (isPause)
                _source.Pause();
            else
                _source.UnPause();
        }

        public void Dispose()
        {
            GameObject.Destroy(_source.gameObject);
        }

        private async void SetNextClip()
        {
            _currentClipIndex++;
            if(_clipReference.Clips.Length == _currentClipIndex)
            {
                if (_clipReference.IsLoop)
                {
                    _currentClipIndex = 0;
                }
                else
                {
                    _source.GetComponent<AudioSourceEventModule>().OnEndPlay -= SetNextClip;
                    return;
                }
            }

            AssetReferenceAudioClip clipReference = _clipReference.Clips[_currentClipIndex];
            _source.clip = await clipReference.GetOrLoad();
            _source.Play();
        }
    }
    public class AudioSourceEventModule : MonoBehaviour
    {
        private AudioSource _source;

        public event Action OnEndPlay;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
        }
        private void Update()
        {
            if (_source.time > _source.clip.length)
                OnEndPlay?.Invoke();                
        }
    }
    public class AudioService
    {
        private Dictionary<AudioGroupType, AudioBus> _audioBuses;

        private List<SourceInstance> _sourceInstances;

        private class AudioBus
        {
            public AudioMixerGroup Group;
            public string VolumeParamter;
            public AudioSource Source;
        }

        public AudioService(AudioServiceConfig config) 
        {
            _sourceInstances = new List<SourceInstance>();
            _audioBuses = new Dictionary<AudioGroupType, AudioBus>(config.TypeGroups.Length);
            foreach(var key in config.TypeGroups)
            {
                AudioSource source = new GameObject(Enum.GetName(typeof(AudioGroupType), key.Type) + "AudioSource").AddComponent<AudioSource>();
                source.outputAudioMixerGroup = key.Group;
                _audioBuses.Add(key.Type, new AudioBus() { Group = key.Group, Source = source, VolumeParamter = key.VolumeProperty });
            }
        }

        public async void PlayOneShot(ClipEvent clipEvent)
        {
            if (clipEvent.Clips.Length == 1)
            {
                _audioBuses[clipEvent.AudioGroup].Source.PlayOneShot(await clipEvent.Clips[0].GetOrLoad());
            }
        }
        public void PlayOneShotOnPoint(ClipEvent clipEvent, Vector3 position) 
        {
            AudioSource audioSource = CreateAudioSource($"{clipEvent.name}AudioSource");
            audioSource.spatialBlend = 1;
            audioSource.transform.position = position;
            SourceInstance sourceInstance = new SourceInstance(audioSource, clipEvent);
            sourceInstance.Play();
            _sourceInstances.Add(sourceInstance);
        }

        public SourceInstance PlayWithSource(ClipEvent clipEvent)
        {
            AudioSource audioSource = CreateAudioSource($"{clipEvent.name}AudioSource");
            SourceInstance sourceInstance = new SourceInstance(audioSource, clipEvent);
            sourceInstance.Play();
            _sourceInstances.Add(sourceInstance);
            return sourceInstance;
        }
        public SourceInstance PlayWithSourceOnPoint(ClipEvent clipEvent, Vector3 position)
        {
            AudioSource audioSource = CreateAudioSource($"{clipEvent.name}AudioSource");
            audioSource.spatialBlend = 1;
            audioSource.transform.position = position;
            SourceInstance sourceInstance = new SourceInstance(audioSource, clipEvent);
            sourceInstance.Play();
            _sourceInstances.Add(sourceInstance);
            return sourceInstance;
        }
        public void RemoveSource(SourceInstance sourceInstance)
        {
            sourceInstance.Stop();
            _sourceInstances.Remove(sourceInstance);
            sourceInstance.Dispose();
        }

        public void PauseAll(bool isPause)
        {
            if (isPause)
            {
                foreach (var source in _audioBuses)
                    source.Value.Source.Pause();
            }
            else
            {
                foreach (var source in _audioBuses)
                    source.Value.Source.UnPause();
            }
            foreach (var instance in _sourceInstances)
                instance.Pause(isPause);
        }
        public void PauseByGroup(AudioGroupType groupType, bool isPause)
        {
            if (isPause)
                _audioBuses[groupType].Source.Pause();
            else
                _audioBuses[groupType].Source.UnPause();
        }

        public float GetVolume(AudioGroupType groupType)
        {
            _audioBuses[groupType].Group.audioMixer.GetFloat(_audioBuses[groupType].VolumeParamter, out float value);
            return value;
        }
        public void SetVolume(AudioGroupType groupType, float value)
        {
            _audioBuses[groupType].Group.audioMixer.SetFloat(_audioBuses[groupType].VolumeParamter, value);
        }

        private AudioSource CreateAudioSource(string name)
        {
            return new GameObject(name).AddComponent<AudioSource>();
        }
    }
}
