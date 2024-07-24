using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Infrastructure.Service
{
    public enum BackgroundMusicType
    {
        MainMenu,
        LoadingScreen,
        Gameplay
    }
    public class BackgroundAudioService : IDisposable
    {
        private AudioSource _backgroundSoundSource;
        private AudioListByType[] _audioClips;

        private string _volumeParameterName;
        private AudioMixerGroup _musicGroup;

        private BackgroundMusicType _currentMusicType;
        private int _currentClipIndex;

        [Serializable]
        public struct AudioListByType
        {
#if UNITY_EDITOR
            public string InspectorName;
#endif
            public BackgroundMusicType Type;
            public AssetReferenceAudioClip[] Clips;
        }

        public BackgroundAudioService(BackgroundAudioServiceConfig config)
        {

            _backgroundSoundSource = new GameObject("BackgroundAudioServiceSource").AddComponent<AudioSource>();
            _audioClips = config.AudioClips;
            _volumeParameterName = config.VolumeParameterName;
            _musicGroup = config.MusicGroup;
            _backgroundSoundSource.outputAudioMixerGroup = _musicGroup;
        }
        void IDisposable.Dispose()
        {
            foreach (AudioListByType list in _audioClips)
                UnloadMusicByType(list.Type);
            GameObject.Destroy(_backgroundSoundSource);
        }

        public async void PlayBackgroundMusicByType(BackgroundMusicType backgroundMusicType)
        {
            if(_currentMusicType != backgroundMusicType)
            {
                UnloadMusicByType(_currentMusicType);
                _currentMusicType = backgroundMusicType;
                _currentClipIndex = 0;
            }
            
            foreach (AudioListByType list in _audioClips)
            {
                if (list.Type != _currentMusicType)
                    continue;
                if (_currentClipIndex >= list.Clips.Length)
                    _currentClipIndex = 0;

                if (list.Clips[_currentClipIndex].Asset == null)
                {
                    if (!list.Clips[_currentClipIndex].IsDone && list.Clips[_currentClipIndex].OperationHandle.PercentComplete < 1f)
                        await list.Clips[_currentClipIndex].OperationHandle.Task;
                    else
                        await list.Clips[_currentClipIndex].LoadAssetAsync().Task;
                }

                _backgroundSoundSource.clip = list.Clips[_currentClipIndex].Asset as AudioClip;
                _backgroundSoundSource.Play();
                _currentMusicType++;
                break;
            }
        }
        public void PauseBackgroundMusic()
        {
            _backgroundSoundSource.Pause();
        }
        public void UnPauseBackgroundMusic()
        {
            _backgroundSoundSource.UnPause();
        }
        public void SetVolume(float volume)
        {
            _musicGroup.audioMixer.SetFloat(_volumeParameterName, volume);
        }

        private void UnloadMusicByType(BackgroundMusicType backgroundMusicType)
        {
            foreach (AudioListByType list in _audioClips)
            {
                if (list.Type != _currentMusicType)
                    continue;

                for(int i = 0; i < list.Clips.Length; i++)
                    list.Clips[i].ReleaseAsset();
                break;
            }
        }
    }
}
