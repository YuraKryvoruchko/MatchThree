using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;
using Cysharp.Threading.Tasks;
using ModestTree;

namespace Core.Infrastructure.Service
{
    [Serializable]
    public struct AudioPath
    {
        public AudioFileType Type;
        public string Key;
    }
    public class AudioService : IInitializable, IDisposable
    {
        private AudioMixerGroup _masterGroup;
        private AudioMixerGroup _musicGroup;
        private AudioMixerGroup _soundGroup;

        private Dictionary<AudioFileType, AudioFile> _audioFileDictionary;
        private Dictionary<AudioFileType, AudioSource> _audioSourceDictionary;

        private Transform _sourceContainer;

        private readonly string _masterVolumeParameter;
        private readonly string _musicVolumeParameter;
        private readonly string _soundVolumeParameter;

        private const float MAX_VOLUME = 0F;
        private const float MIN_VOLUME = -80F;

        public AudioService(AudioServiceConfig config)
        {
            _audioFileDictionary = new Dictionary<AudioFileType, AudioFile>();
            _audioSourceDictionary = new Dictionary<AudioFileType, AudioSource>();
            _masterGroup = config.MasterGroup;
            _musicGroup = config.MusicGroup;
            _soundGroup = config.SoundGroup;
            _masterVolumeParameter = config.MasterVolumeParameter;
            _musicVolumeParameter = config.MusicVolumeParameter;
            _soundVolumeParameter = config.SoundVolumeParameter;
        }

        void IInitializable.Initialize()
        {
            _sourceContainer = new GameObject("AudioServiceContainer").transform;
        }
        void IDisposable.Dispose() 
        {
            foreach (var pair in _audioSourceDictionary)
                GameObject.Destroy(pair.Value.gameObject);
            foreach (var pair in _audioFileDictionary)
                UnLoadAudioFile(pair.Key);

            _audioSourceDictionary.Clear();
            _audioFileDictionary.Clear();
        }

        public async UniTask LoadAudioFile(AudioFile file)
        {
            _audioFileDictionary.Add(file.Type, file);

            List<UniTask> tasks = new List<UniTask>();
            foreach (AudioFile.ClipKey clip in file.AudioClips)
            {
                if (clip.LoadImmediately)
                    tasks.Add(clip.AudioClip.LoadAssetAsync().ToUniTask());
            }
            await UniTask.WhenAll(tasks);

            AudioSource source = new GameObject($"{file.Type}AudioSource").AddComponent<AudioSource>();
            source.transform.SetParent(_sourceContainer);
            source.outputAudioMixerGroup = GetGroup(file.GroupType);
            source.playOnAwake = false;
            _audioSourceDictionary.Add(file.Type, source);
        }
        public void UnLoadAudioFile(AudioFileType fileType)
        {
            AudioFile file = _audioFileDictionary[fileType];
            foreach (AudioFile.ClipKey clip in file.AudioClips)
            {
                clip.AudioClip.ReleaseAsset();
            }
            _audioFileDictionary.Remove(file.Type);
            GameObject.Destroy(_audioSourceDictionary.GetValueAndRemove(file.Type));
        }

        public async void Play(AudioPath audioPath, float volume = 1f)
        {
            AudioFile file = _audioFileDictionary[audioPath.Type];
            for(int i = 0; i < file.AudioClips.Length; i++)
            {
                if (file.AudioClips[i].Key != audioPath.Key)
                    continue;

                AudioSource source = _audioSourceDictionary[file.Type];
                source.clip = await file.AudioClips[i].AudioClip.GetOrLoad();
                source.volume = volume;
                source.Play();
            }
        }
        public async void PlayOneShot(AudioPath audioPath, float volume = 1f)
        {
            AudioFile file = _audioFileDictionary[audioPath.Type];
            for (int i = 0; i < file.AudioClips.Length; i++)
            {
                if (file.AudioClips[i].Key != audioPath.Key)
                    continue;

                AudioSource source = _audioSourceDictionary[file.Type];
                source.PlayOneShot(await file.AudioClips[i].AudioClip.GetOrLoad(), volume);
            }
        }

        public void SetPause(AudioFileType type, bool isPause)
        {
            if(isPause)
                _audioSourceDictionary[type].Pause();
            else
                _audioSourceDictionary[type].UnPause();
        }

        public void MuteGroup(AudioGroupType type, bool isMute)
        {
            if (isMute)
                SetVolume(type, MIN_VOLUME);
            else
                SetVolume(type, MAX_VOLUME);
        }
        public void SetVolume(AudioGroupType type, float volume)
        {
            AudioMixerGroup mixerGroup = GetGroup(type);
            mixerGroup.audioMixer.SetFloat(GetVolumeParameter(type), volume);
        }
        public float GetVolume(AudioGroupType type)
        {
            AudioMixerGroup mixerGroup = GetGroup(type);
            float volume = 0;
            mixerGroup.audioMixer.GetFloat(GetVolumeParameter(type), out volume);
            return volume;
        }

        private AudioMixerGroup GetGroup(AudioGroupType audioGroupType)
        {
            switch (audioGroupType)
            {
                case AudioGroupType.Master:
                    return _masterGroup;
                case AudioGroupType.Music:
                    return _musicGroup;
                case AudioGroupType.Sound:
                    return _soundGroup;
                default:
                    throw new Exception($"This {audioGroupType} group is not implemented!");
            }
        }
        private string GetVolumeParameter(AudioGroupType audioGroupType)
        {
            switch (audioGroupType)
            {
                case AudioGroupType.Master:
                    return _masterVolumeParameter;
                case AudioGroupType.Music:
                    return _musicVolumeParameter;
                case AudioGroupType.Sound:
                    return _soundVolumeParameter;
                default:
                    throw new Exception($"This {audioGroupType} group is not implemented!");
            }
        }
    }
}
