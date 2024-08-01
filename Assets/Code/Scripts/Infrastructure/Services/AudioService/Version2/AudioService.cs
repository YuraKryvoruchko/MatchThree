using Core.Infrastructure.Service;
using System;
using System.Collections.Generic;
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
    public class AudioService : IDisposable
    {
        private Dictionary<AudioGroupType, AudioBus> _audioBuses;
        private List<SourceInstance> _sourceInstances;

        private Transform _sourceContainer;

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
            _sourceContainer = new GameObject("AudioSourceContainer").transform;
            foreach (var key in config.TypeGroups)
            {
                AudioSource source = CreateAudioSource($"{Enum.GetName(typeof(AudioGroupType), key.Type)}AudioSource", key.Group);
                _audioBuses.Add(key.Type, new AudioBus() { Group = key.Group, Source = source, VolumeParamter = key.VolumeProperty });
            }
        }

        void IDisposable.Dispose()
        {
            foreach (SourceInstance instance in _sourceInstances)
                ReleaseSource(instance);
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
            AudioSource audioSource = CreateAudioSource($"{clipEvent.name}AudioSource", _audioBuses[clipEvent.AudioGroup].Group);
            audioSource.spatialBlend = 1;
            audioSource.transform.position = position;
            SourceInstance sourceInstance = new SourceInstance(audioSource, clipEvent);
            sourceInstance.Play();
            _sourceInstances.Add(sourceInstance);
        }

        public SourceInstance PlayWithSource(ClipEvent clipEvent)
        {
            AudioSource audioSource = CreateAudioSource($"{clipEvent.name}AudioSource", _audioBuses[clipEvent.AudioGroup].Group);
            SourceInstance sourceInstance = new SourceInstance(audioSource, clipEvent);
            sourceInstance.Play();
            _sourceInstances.Add(sourceInstance);
            return sourceInstance;
        }
        public SourceInstance PlayWithSourceOnPoint(ClipEvent clipEvent, Vector3 position)
        {
            AudioSource audioSource = CreateAudioSource($"{clipEvent.name}AudioSource", _audioBuses[clipEvent.AudioGroup].Group);
            audioSource.spatialBlend = 1;
            audioSource.transform.position = position;
            SourceInstance sourceInstance = new SourceInstance(audioSource, clipEvent);
            sourceInstance.Play();
            _sourceInstances.Add(sourceInstance);
            return sourceInstance;
        }
        public void ReleaseSource(SourceInstance sourceInstance)
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

        private AudioSource CreateAudioSource(string name, AudioMixerGroup group)
        {
            AudioSource source = new GameObject(name).AddComponent<AudioSource>();
            source.transform.parent = _sourceContainer.transform;
            source.playOnAwake = false;
            source.outputAudioMixerGroup = group;
            return source;
        }
    }
}
