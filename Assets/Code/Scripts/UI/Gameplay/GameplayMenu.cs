using System;
using UnityEngine;
using UnityEngine.UI;
using Core.Infrastructure.UI;
using Core.Infrastructure.Service;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Service.Pause;
using Zenject;

namespace Core.UI.Gameplay
{
    public class GameplayMenu : WindowBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _pauseMenu;
        [Header("Modules")]
        [SerializeField] private UIAbilityHolder _uiAbilityHolder;
        [Header("Popups")]
        [SerializeField] private WindowBase _popupPrefab;
        [Header("Audio Keys")]
        [SerializeField] private ClipEvent _uiClickKey;

        private IWindowService _windowService;
        private IPauseService _pauseService;
        private IAudioService _audioService;

        private WindowBase _pausePopup;

        public override event Action OnMenuBack;

        [Inject]
        private void Construct(IWindowService windowService, IPauseService pauseService, IAudioService audioService)
        {
            _windowService = windowService;
            _pauseService = pauseService;
            _audioService = audioService;
        }

        protected override void OnShow()
        {
            _pauseMenu.onClick.AddListener(CreateMenuPopup);
        }
        protected override void OnHide()
        {
            _pauseMenu.onClick.RemoveListener(CreateMenuPopup);
        }
        protected override void OnFocus()
        {
            _pauseMenu.interactable = true;
            _uiAbilityHolder.SetInteractable(true);
        }
        protected override void OnUnfocus()
        {
            _pauseMenu.interactable = false;
            _uiAbilityHolder.SetInteractable(false);
        }
        protected override void OnClose()
        {
            OnHide();
            OnUnfocus();
        }

        private async void CreateMenuPopup()
        {
            _pauseService.SetPause(true);
            _audioService.PlayOneShot(_uiClickKey);
            _pausePopup = await _windowService.OpenPopup<WindowBase>(_popupPrefab.Path);
            _pausePopup.OnMenuBack += HandlePausePopupClosing;
        }
        private void HandlePausePopupClosing()
        {
            _pausePopup.OnMenuBack -= HandlePausePopupClosing;
            _pauseService.SetPause(false);
        }
    }
}
