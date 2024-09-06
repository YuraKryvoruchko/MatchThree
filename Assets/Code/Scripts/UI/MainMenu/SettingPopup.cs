using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using TMPro;
using Core.Infrastructure.UI;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Service.Saving;

namespace Core.UI.Menu
{
    public class SettingPopup : WindowBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _closeButton;
        [Header("Switchs")]
        [SerializeField] private SwitchButton _musicSwitch;
        [SerializeField] private SwitchButton _soundSwitch;
        [Header("Dropdown")]
        [SerializeField] private TMP_Dropdown _frameRateDropDown;
        [Header("Audio Events")]
        [SerializeField] private ClipEvent _clickAudioEvent;
        [SerializeField] private ClipEvent _switchAudiohEvent;

        private IAudioService _audioService;

        public override event Action OnMenuBack;

        private const int MIN_FRAME_RATE = 30;

        [Inject]
        private void Construct(IAudioService audioService)
        {
            _audioService = audioService;
        }
        private void Awake()
        {
            SetupDropdown();
            SetupVolumeSwitches();
        }

        protected override void OnShow()
        {
            _closeButton.onClick.AddListener(() => OnMenuBack?.Invoke());
            _closeButton.onClick.AddListener(() => _audioService.PlayOneShot(_clickAudioEvent));
            _musicSwitch.Button.onClick.AddListener(() => _audioService.PlayOneShot(_switchAudiohEvent));
            _musicSwitch.Button.onClick.AddListener(() => SwitchTypeSoundVolume(_musicSwitch, AudioGroupType.Music));
            _soundSwitch.Button.onClick.AddListener(() => _audioService.PlayOneShot(_switchAudiohEvent));
            _soundSwitch.Button.onClick.AddListener(() => SwitchTypeSoundVolume(_soundSwitch, AudioGroupType.Sound));
        }
        protected override void OnHide()
        {
            _closeButton.onClick.RemoveAllListeners();
            _musicSwitch.Button.onClick.RemoveAllListeners();
            _soundSwitch.Button.onClick.RemoveAllListeners();
        }
        protected override void OnFocus()
        {
            SetInteractable(true);
        }
        protected override void OnUnfocus()
        {
            SetInteractable(false);
        }
        protected override void OnClose()
        {
        }

        private void SetInteractable(bool interactable)
        {
            _closeButton.interactable = interactable;
            _musicSwitch.Button.interactable = interactable;
            _soundSwitch.Button.interactable = interactable;
        }

        private void SetupDropdown()
        {
            int refreshRate = (int)Math.Round(Screen.currentResolution.refreshRateRatio.value);
            for (int i = 1; refreshRate / i >= MIN_FRAME_RATE; i++)
            {
                _frameRateDropDown.options.Add(new TMP_Dropdown.OptionData((refreshRate / i).ToString()));
                if (Application.targetFrameRate == refreshRate / i)
                    _frameRateDropDown.SetValueWithoutNotify(i - 1);
            }

            _frameRateDropDown.onValueChanged.AddListener(ChangeFrameRate);
        }
        private void SetupVolumeSwitches()
        {
            _musicSwitch.SetActive(!(_audioService.GetVolume(AudioGroupType.Music) < 0f));
            _soundSwitch.SetActive(!(_audioService.GetVolume(AudioGroupType.Sound) < 0f));
        }

        private void SwitchTypeSoundVolume(SwitchButton switchButton, AudioGroupType audioGroupType)
        {
            float nextVolumeValue;
            if (_audioService.GetVolume(audioGroupType) < 0f)
                nextVolumeValue = 0f;
            else
                nextVolumeValue = -80f;

            _audioService.SetVolume(audioGroupType, nextVolumeValue);

            if(audioGroupType == AudioGroupType.Music)
                PlayerPrefs.SetFloat(PlayerPrefsEnum.AudioSettings.MUSIC_VOLUME_VALUE_KEY, nextVolumeValue);
            else
                PlayerPrefs.SetFloat(PlayerPrefsEnum.AudioSettings.SOUND_VOLUME_VALUE_KEY, nextVolumeValue);
            PlayerPrefs.Save();

            switchButton.Switch();
        }
        private void ChangeFrameRate(int frameRateIndex)
        {
            int refreshRate = (int)Math.Round(Screen.currentResolution.refreshRateRatio.value);
            Application.targetFrameRate = refreshRate / (frameRateIndex + 1);
        }
    }
}
