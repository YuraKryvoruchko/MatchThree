using UnityEngine;
using Zenject;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Service.Saving;

namespace Core.Infrastructure.Boot
{
    public class SetupAudioService : IInitializable
    {
        private IAudioService _audioService;

        private const float DEFAULT_VOLUME = 0F;

        public SetupAudioService(IAudioService audioService)
        {
            _audioService = audioService;
        }

        public void Initialize()
        {
            if (PlayerPrefs.HasKey(PlayerPrefsEnum.AudioSettings.MUSIC_VOLUME_VALUE_KEY))
            {
                _audioService.SetVolume(AudioGroupType.Music, PlayerPrefs.GetFloat(PlayerPrefsEnum.AudioSettings.MUSIC_VOLUME_VALUE_KEY));
            }
            else
            {
                _audioService.SetVolume(AudioGroupType.Music, DEFAULT_VOLUME);
                PlayerPrefs.SetFloat(PlayerPrefsEnum.AudioSettings.MUSIC_VOLUME_VALUE_KEY, DEFAULT_VOLUME);
            }

            if (PlayerPrefs.HasKey(PlayerPrefsEnum.AudioSettings.SOUND_VOLUME_VALUE_KEY))
            {
                _audioService.SetVolume(AudioGroupType.Sound, PlayerPrefs.GetFloat(PlayerPrefsEnum.AudioSettings.SOUND_VOLUME_VALUE_KEY));
            }
            else
            {
                _audioService.SetVolume(AudioGroupType.Sound, DEFAULT_VOLUME);
                PlayerPrefs.SetFloat(PlayerPrefsEnum.AudioSettings.SOUND_VOLUME_VALUE_KEY, DEFAULT_VOLUME);
            }
        }
    }
}
