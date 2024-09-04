using System;
using UnityEngine;
using UnityEngine.UI;
using Core.Infrastructure.UI;
using Core.Infrastructure.Service;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Service.Pause;
using Zenject;
using Core.Infrastructure.Gameplay;

namespace Core.UI.Gameplay
{
    public class GameplayMenu : WindowBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _pauseMenuButton;
        [Header("Modules")]
        [SerializeField] private UIAbilityHolder _uiAbilityHolder;
        [Header("Popups")]
        [SerializeField] private WindowBase _popupPrefab;
        [Header("Audio Keys")]
        [SerializeField] private ClipEvent _uiClickKey;

        private IWindowService _windowService;
        private IGameModeSimulation _gameModeSimulation;
        private IPauseService _pauseService;
        private IAudioService _audioService;

        private WindowBase _pausePopup;

        private const float SNAPSHOT_CHANGING_DELAY = 0.1F;

        public override event Action OnMenuBack;

        [Inject]
        private void Construct(IWindowService windowService, IGameModeSimulation gameModeSimulation,
            IPauseService pauseService, IAudioService audioService)
        {
            _gameModeSimulation = gameModeSimulation;
            _windowService = windowService;
            _pauseService = pauseService;
            _audioService = audioService;
        }

        private void OnEnable()
        {
            _gameModeSimulation.OnGameComplete += OnUnfocus;
        }
        private void OnDisable()
        {
            _gameModeSimulation.OnGameComplete -= OnUnfocus;
        }

        protected override void OnShow()
        {
            _pauseMenuButton.onClick.AddListener(CreateMenuPopup);
        }
        protected override void OnHide()
        {
            _pauseMenuButton.onClick.RemoveListener(CreateMenuPopup);
        }
        protected override void OnFocus()
        {
            _pauseMenuButton.interactable = true;
            _uiAbilityHolder.SetInteractable(true);
        }
        protected override void OnUnfocus()
        {
            _pauseMenuButton.interactable = false;
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
            _audioService.ChangeSnapshot(AudioSnapshotType.Paused, SNAPSHOT_CHANGING_DELAY);
            _pausePopup = await _windowService.OpenPopup<WindowBase>(_popupPrefab.Path);
            _pausePopup.OnMenuBack += HandlePausePopupClosing;
        }
        private void HandlePausePopupClosing()
        {
            _pausePopup.OnMenuBack -= HandlePausePopupClosing;
            _audioService.ChangeSnapshot(AudioSnapshotType.Default, SNAPSHOT_CHANGING_DELAY);
            _pauseService.SetPause(false);
        }
        private void BackMenu()
        {
            OnMenuBack?.Invoke();
        }
    }
}
