using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using Zenject;

namespace Core.Infrastructure.Service
{
    public interface IAudioService<T> where T : Enum
    {
        float Volume { get; set; }

        void Play(T type);
    }

    public enum AudioGroupType
    {
        Master,
        Music,
        Sound
    }
    public enum AudioFileType
    {
        Background,
        UI,
        Gameplay
    }
    [CreateAssetMenu(fileName = "AudioFile", menuName = "SO/Audio/AudioFile")]
    public class AudioFile : ScriptableObject
    {
        public AudioGroupType GroupType;
        public AudioFileType Type;
        public ClipKey[] AudioClips;

        [Serializable]
        public struct ClipKey
        {
            [Header("Default")]
            public string Key;
            public AssetReferenceAudioClip AudioClip;
            [Header("Properties")]
            public bool LoadImmediately;
        }
    }
    public class AudioService : IInitializable, IDisposable
    {
        private AudioMixerGroup _masterGroup;
        private AudioMixerGroup _musicGroup;
        private AudioMixerGroup _soundGroup;

        private AudioSource _masterSource;
        private AudioSource _musicSource;
        private AudioSource _soundSource;

        private Dictionary<AudioFileType, AudioFile> _audioFileDictionary;

        private const float MAX_VOLUME = 0F;
        private const float MIN_VOLUME = -80F;

        public AudioService()
        {
            _audioFileDictionary = new Dictionary<AudioFileType, AudioFile>();
        }

        void IInitializable.Initialize()
        {
            throw new NotImplementedException();
        }
        void IDisposable.Dispose() 
        {
            throw new NotImplementedException();
        }

        public void LoadAudioFile(AudioFile file)
        {
            _audioFileDictionary.Add(file.Type, file);
            foreach (AudioFile.ClipKey clip in file.AudioClips)
            {
                if (clip.LoadImmediately)
                    clip.AudioClip.LoadAssetAsync();
            }
        }
        public void UnLoadAudioFile(AudioFileType fileType)
        {
            AudioFile file = _audioFileDictionary[fileType];
            foreach (AudioFile.ClipKey clip in file.AudioClips)
            {
                clip.AudioClip.ReleaseAsset();
            }
            _audioFileDictionary.Remove(file.Type);
        }

        public void Play(AudioFileType fileType, string fileKey, float volume = MAX_VOLUME)
        {
            AudioFile file = _audioFileDictionary[fileType];
            for(int i = 0; i < file.AudioClips.Length; i++)
            {
                if (file.AudioClips[i].Key != fileKey)
                    continue;

                AudioSource source = GetSource(file.GroupType);
                throw new NotImplementedException();
            }
        }
        public void PlayOnPoint()
        {

        }
        public void SetPause(bool isPause)
        {

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
            switch(type)
            {
                case AudioGroupType.Master:
                    _masterGroup.audioMixer.SetFloat("m", volume);
                    break;
                case AudioGroupType.Music:
                    _musicGroup.audioMixer.SetFloat("m", volume);
                    break;
                case AudioGroupType.Sound:
                    _soundGroup.audioMixer.SetFloat("m", volume);
                    break;
                default:
                    throw new Exception($"This {type} group is not implemented!");
            }
        }

        private AudioSource GetSource(AudioGroupType type)
        {
            switch (type)
            {
                case AudioGroupType.Master:
                    return _masterSource;
                case AudioGroupType.Music:
                    return _musicSource;
                case AudioGroupType.Sound:
                    return _soundSource;
                default:
                    throw new Exception($"This {type} group is not implemented!");
            }
        }
    }
}
