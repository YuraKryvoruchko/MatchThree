using UnityEngine;
using UnityEngine.Audio;
using com.cyborgAssets.inspectorButtonPro;

namespace Core.Infrastructure.Service
{
    public enum BackgroundMusicType
    {
        MainMenu,
        LoadingScreen,
        Gameplay
    }
    public class BackgroundAudioService : MonoBehaviour
    {
        [Header("Music Settings")]
        [SerializeField] private AudioSource _backgroundSoundSource;
        [SerializeField] private AudioListByType[] _audioClips;
        [Header("Audio Group")]
        [SerializeField] private string _volumeParameterName;
        [SerializeField] private AudioMixerGroup _musicGroup;

        private BackgroundMusicType _currentMusicType;
        private int _currentClipIndex;

        [System.Serializable]
        private struct AudioListByType
        {
#if UNITY_EDITOR
            public string InspectorName;
#endif
            public BackgroundMusicType Type;
            public AssetReferenceAudioClip[] Clips;
        }

        [ProPlayButton]
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

                if(list.Clips[_currentClipIndex].Asset == null)
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
        [ProPlayButton]
        public void PauseBackgroundMusic()
        {
            _backgroundSoundSource.Pause();
        }
        [ProPlayButton]
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
