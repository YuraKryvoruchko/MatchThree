using UnityEngine;
using UnityEngine.Audio;
using com.cyborgAssets.inspectorButtonPro;

namespace Core.Infrastructure.Service
{
    public enum AudioType
    {
        Music,
        Sound,
        UI
    }
    public enum BackgroundMusicType
    {
        MainMenu,
        LoadingScreen,
        Gameplay
    }
    public enum UISound
    {
        Click,
        Switch
    }
    public class AudioService : MonoBehaviour
    {
        [Header("Music Settings")]
        [SerializeField] private AudioSource _backgroundSoundSource;
        [SerializeField] private AudioListByType[] _audioClips;
        [Header("UI Sound Settings")]
        [SerializeField] private AudioSource _uiSoundSource;
        [SerializeField] private AssetReferenceAudioClip _clickClip;
        [SerializeField] private AssetReferenceAudioClip _switchClip;
        [Header("Audio Groups")]
        [SerializeField] private AudioMixerGroup _musicGroup;
        [SerializeField] private AudioMixerGroup _soundGroup;
        [SerializeField] private AudioMixerGroup _uiGroup;

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
        public void SetBackgroundMusicMuteMode(bool isMute)
        {
            _backgroundSoundSource.mute = isMute;
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
