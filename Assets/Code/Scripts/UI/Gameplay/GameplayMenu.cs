using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using Zenject;
using Core.Infrastructure.UI;
using Core.Infrastructure.Service;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Service.Pause;
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
        [SerializeField] private AssetReferenceGameObject _pausePopupReference;
        [SerializeField] private AssetReferenceGameObject _completePopupReference;
        [Header("Audio Keys")]
        [SerializeField] private ClipEvent _uiClickKey;

        private IWindowService _windowService;
        private IGameModeSimulation _gameModeSimulation;
        private IPauseService _pauseService;
        private IAudioService _audioService;

        private WindowBase _pausePopup;
        private WindowBase _completePopup;

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
            _gameModeSimulation.OnBlockGame += OnUnfocus;
            _gameModeSimulation.OnGameComplete += CreateCompletePopup;
        }
        private void OnDisable()
        {
            _gameModeSimulation.OnBlockGame -= OnUnfocus;
            _gameModeSimulation.OnGameComplete -= CreateCompletePopup;
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
            _pausePopup = await _windowService.OpenPopup<WindowBase>(_pausePopupReference.AssetGUID);
            _pausePopup.OnMenuBack += HandlePausePopupClosing;
        }
        private async void CreateCompletePopup()
        {
            _pauseService.SetPause(true);
            CompletePopup completePopup = await _windowService.OpenPopup<CompletePopup>(_completePopupReference.AssetGUID);
            _completePopup = completePopup;
            completePopup.Activate().Forget();
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
