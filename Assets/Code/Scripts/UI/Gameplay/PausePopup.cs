using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Core.Infrastructure.Service;
using Core.Infrastructure.UI;

namespace Core.UI.Gameplay
{
    public class PausePopup : WindowBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _replayButton;
        [SerializeField] private Button _quitButton;
        [Header("Switched Button")]
        [SerializeField] private SwitchButton _musicButton;
        [SerializeField] private SwitchButton _soundButton;

        private UIAudioService _uiAudioService;
        private BackgroundAudioService _backgroundAudioService;

        public override event Action OnMenuBack;

        [Inject]
        private void Construct(BackgroundAudioService backgroundAudioService, UIAudioService uiAudioService)
        {
            _uiAudioService = uiAudioService;
            _backgroundAudioService = backgroundAudioService;
        }

        protected override void OnShow()
        {
            _closeButton.onClick.AddListener(BackMenu);
            _closeButton.onClick.AddListener(ClickSound);
            _replayButton.onClick.AddListener(ClickSound);
            _quitButton.onClick.AddListener(ClickSound);
            _musicButton.Button.onClick.AddListener(SwitchBackgroundMusicVolume);
            _musicButton.Button.onClick.AddListener(SwitchSound);
            _soundButton.Button.onClick.AddListener(SwitchSoundsVolume);
            _soundButton.Button.onClick.AddListener(SwitchSound);

            _musicButton.SetActive(!(_backgroundAudioService.GetVolume() < 0f));
            _soundButton.SetActive(!(_uiAudioService.GetVolume() < 0f));
        }
        protected override void OnHide()
        {
            _closeButton.onClick.RemoveAllListeners();
            _replayButton.onClick.RemoveAllListeners();
            _quitButton.onClick.RemoveAllListeners();
            _musicButton.Button.onClick.RemoveAllListeners();
            _soundButton.Button.onClick.RemoveAllListeners();
        }
        protected override void OnFocus()
        {
            _closeButton.interactable = true;
            _quitButton.interactable = true;
            _closeButton.interactable = true;
            _musicButton.Button.interactable = true;
            _soundButton.Button.interactable = true;
        }
        protected override void OnUnfocus()
        {
            _closeButton.interactable = false;
            _quitButton.interactable = false;
            _closeButton.interactable = false;
            _musicButton.Button.interactable = false;
            _soundButton.Button.interactable = false;
        }
        protected override void OnClose()
        {
            OnHide();
            OnUnfocus();
        }

        private void BackMenu()
        {
            OnMenuBack?.Invoke();
        }

        private void SwitchBackgroundMusicVolume()
        {
            if (_backgroundAudioService.GetVolume() < 0f)
                _backgroundAudioService.SetVolume(0f);
            else
                _backgroundAudioService.SetVolume(-80f);
            _musicButton.Switch();
        }
        private void SwitchSoundsVolume()
        {
            if(_uiAudioService.GetVolume() < 0)
                _uiAudioService.SetVolume(0f);
            else
                _uiAudioService.SetVolume(-80f);
            _soundButton.Switch();
        }

        private void ClickSound()
        {
            _uiAudioService.PlaySound(UISoundType.Click);
        }
        private void SwitchSound()
        {
            _uiAudioService.PlaySound(UISoundType.Switch);
        }
    }
}
