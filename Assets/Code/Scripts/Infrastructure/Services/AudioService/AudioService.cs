using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Core.Infrastructure.Service.Audio
{
    public enum AudioGroupType
    {
        Master,
        Music,
        Sound
    }
    public enum AudioSnapshotType
    {
        Default,
        Paused
    }
    public class AudioService : IAudioService, IInitializable, IDisposable
    {
        private Dictionary<AudioGroupType, GroupAudioComponent> _audioBuses;
        private Dictionary<AudioSnapshotType, AudioMixerSnapshot> _snapshots;

        private List<SourceInstance> _sourceInstances;

        private Transform _sourceContainer;

        private AudioServiceConfig _config;

        private class GroupAudioComponent
        {
            public AudioMixerGroup Group;
            public string VolumeParamter;
            public AudioSource Source;
        }

        public AudioService(AudioServiceConfig config) 
        {
            _sourceInstances = new List<SourceInstance>();
            _audioBuses = new Dictionary<AudioGroupType, GroupAudioComponent>(config.TypeGroups.Length);
            _snapshots = new Dictionary<AudioSnapshotType, AudioMixerSnapshot>();
            _config = config;
        }

        void IInitializable.Initialize() 
        {
            _sourceContainer = new GameObject("AudioSourceContainer").transform;
            foreach (var groupKey in _config.TypeGroups)
            {
                AudioSource source = CreateAudioSource($"{Enum.GetName(typeof(AudioGroupType), groupKey.Type)}AudioSource", groupKey.Group);
                _audioBuses.Add(groupKey.Type, new GroupAudioComponent() { Group = groupKey.Group, Source = source, VolumeParamter = groupKey.VolumeProperty });
            }
            foreach (var snapshotKey in _config.TypeSnapshots)
            {
                _snapshots.Add(snapshotKey.Type, snapshotKey.Snapshot);
            }
        }
        void IDisposable.Dispose()
        {
            foreach (SourceInstance instance in _sourceInstances)
                ReleaseSource(instance);
        }

        public void PlayOneShot(ClipEvent clipEvent)
        {
            if (clipEvent.Clips.Length == 1)
            {
                clipEvent.Clips[0].AudioClip.GetOrLoad().ContinueWith((clip) =>
                {
                    _audioBuses[clipEvent.AudioGroup].Source.PlayOneShot(clip);
                    clipEvent.Clips[0].AudioClip.ReleaseAsset();
                }).Forget();
            }
        }
        public void PlayOneShotOnPoint(ClipEvent clipEvent, Vector3 position) 
        {
            AudioSource audioSource = CreateAudioSource($"{clipEvent.name}AudioSource", _audioBuses[clipEvent.AudioGroup].Group);
            audioSource.spatialBlend = 1;
            audioSource.transform.position = position;
            SourceInstance sourceInstance = new SourceInstance(audioSource, clipEvent);
            sourceInstance.OnEndPlaying += HandleEndSourceInstancePlaying;
            sourceInstance.Play();
            _sourceInstances.Add(sourceInstance);
        }

        public SourceInstance PlayWithSource(ClipEvent clipEvent, bool playOnAwake = true)
        {
            AudioSource audioSource = CreateAudioSource($"{clipEvent.name}AudioSource", _audioBuses[clipEvent.AudioGroup].Group);
            SourceInstance sourceInstance = new SourceInstance(audioSource, clipEvent);

            if(playOnAwake)
                sourceInstance.Play();

            _sourceInstances.Add(sourceInstance);
            return sourceInstance;
        }
        public SourceInstance PlayWithSourceOnPoint(ClipEvent clipEvent, Vector3 position, bool playOnAwake = true)
        {
            AudioSource audioSource = CreateAudioSource($"{clipEvent.name}AudioSource", _audioBuses[clipEvent.AudioGroup].Group);
            audioSource.spatialBlend = 1;
            audioSource.transform.position = position;
            SourceInstance sourceInstance = new SourceInstance(audioSource, clipEvent);

            if (playOnAwake)
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

        public void ChangeSnapshot(AudioSnapshotType type, float timeToReach = 0f)
        {
            _snapshots[type].TransitionTo(timeToReach);
        }

        private AudioSource CreateAudioSource(string name, AudioMixerGroup group)
        {
            AudioSource source = new GameObject(name).AddComponent<AudioSource>();
            source.transform.parent = _sourceContainer.transform;
            source.playOnAwake = false;
            source.outputAudioMixerGroup = group;
            return source;
        }
        private void HandleEndSourceInstancePlaying(SourceInstance sourceInstance)
        {
            sourceInstance.OnEndPlaying -= HandleEndSourceInstancePlaying;
            sourceInstance.Dispose();
            _sourceInstances.Remove(sourceInstance);
        }
    }
}
