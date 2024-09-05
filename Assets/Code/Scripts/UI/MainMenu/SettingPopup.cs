using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Core.Infrastructure.UI;
using Core.Infrastructure.Service.Audio;

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
        [SerializeField] private Dropdown _frameRateDropDown;
        [Header("Audio Events")]
        [SerializeField] private ClipEvent _clickAudioEvent;
        [SerializeField] private ClipEvent _switchAudiohEvent;

        private IAudioService _audioService;

        public override event Action OnMenuBack;

        [Inject]
        private void Construct(IAudioService audioService)
        {
            _audioService = audioService;
        }

        protected override void OnShow()
        {
            _closeButton.onClick.AddListener(() => OnMenuBack?.Invoke());
            _closeButton.onClick.AddListener(() => _audioService.PlayOneShot(_clickAudioEvent));
        }
        protected override void OnHide()
        {
            _closeButton.onClick.RemoveAllListeners();
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
        }

        private void ChangeFrameRate()
        {
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
        }
    }
}
