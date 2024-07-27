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
        [Header("Audio Keys")]
        [SerializeField] private AudioPath _uiClickKey;
        [SerializeField] private AudioPath _uiSwitchKey;

        private AudioService _audioService;

        public override event Action OnMenuBack;

        [Inject]
        private void Construct(AudioService audioService)
        {
            _audioService = audioService;
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

            _musicButton.SetActive(!(_audioService.GetVolume(AudioGroupType.Music) < 0f));
            _soundButton.SetActive(!(_audioService.GetVolume(AudioGroupType.Sound) < 0f));
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
            if (_audioService.GetVolume(AudioGroupType.Music) < 0f)
                _audioService.MuteGroup(AudioGroupType.Music, false);
            else
                _audioService.MuteGroup(AudioGroupType.Music, true);

            _musicButton.Switch();
        }
        private void SwitchSoundsVolume()
        {
            if (_audioService.GetVolume(AudioGroupType.Sound) < 0f)
                _audioService.MuteGroup(AudioGroupType.Sound, false);
            else
                _audioService.MuteGroup(AudioGroupType.Sound, true);

            _soundButton.Switch();
        }

        private void ClickSound()
        {
            _audioService.PlayOneShot(_uiClickKey);
        }
        private void SwitchSound()
        {
            _audioService.PlayOneShot(_uiSwitchKey);
        }
    }
}
