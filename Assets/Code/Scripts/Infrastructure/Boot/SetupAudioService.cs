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
            if (PlayerPrefs.HasKey(SettingKeysEnum.MUSIC_VOLUME_VALUE_KEY))
            {
                _audioService.SetVolume(AudioGroupType.Music, PlayerPrefs.GetFloat(SettingKeysEnum.MUSIC_VOLUME_VALUE_KEY));
            }
            else
            {
                _audioService.SetVolume(AudioGroupType.Music, DEFAULT_VOLUME);
                PlayerPrefs.SetFloat(SettingKeysEnum.MUSIC_VOLUME_VALUE_KEY, DEFAULT_VOLUME);
            }

            if (PlayerPrefs.HasKey(SettingKeysEnum.SOUND_VOLUME_VALUE_KEY))
            {
                _audioService.SetVolume(AudioGroupType.Sound, PlayerPrefs.GetFloat(SettingKeysEnum.SOUND_VOLUME_VALUE_KEY));
            }
            else
            {
                _audioService.SetVolume(AudioGroupType.Sound, DEFAULT_VOLUME);
                PlayerPrefs.SetFloat(SettingKeysEnum.SOUND_VOLUME_VALUE_KEY, DEFAULT_VOLUME);
            }
        }
    }
}
