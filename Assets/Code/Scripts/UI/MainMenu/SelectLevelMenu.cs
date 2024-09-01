using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Core.Infrastructure.Service;
using Core.Infrastructure.UI;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Loading;
using Core.Infrastructure.Service.Saving;

namespace Core.UI
{
    public class SelectLevelMenu : WindowBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _backButton;
        [Header("Select Button Settings")]
        [SerializeField] private SelectLevelButton _selectLevelButtonPrefab;
        [SerializeField] private Transform _buttonContainer;
        [Header("Audio Keys")]
        [SerializeField] private ClipEvent _clickAudioEvent;

        private SelectLevelButton[] _buttons;

        private ILoadingScreenProvider _loadingScreenProvider;

        private SceneService _sceneService;
        private IAudioService _audioService;
        private ILevelService _levelService;
        private ISavingService _savingService;

        public override event Action OnMenuBack;

        [Inject]
        private void Construct(SceneService sceneService, IAudioService audioService, ILevelService levelService, ISavingService savingService,
            ILoadingScreenProvider loadingScreenProvider)
        {
            _sceneService = sceneService;
            _audioService = audioService;
            _levelService = levelService;
            _savingService = savingService;
            _loadingScreenProvider = loadingScreenProvider;
        }

        protected override void OnShow()
        {
            _backButton.onClick.AddListener(() => OnMenuBack?.Invoke());
        }
        protected override void OnHide()
        {
            _backButton.onClick.RemoveAllListeners();
        }
        protected override void OnFocus()
        {
            _backButton.interactable = true;
        }
        protected override void OnUnfocus()
        {
            _backButton.interactable = false;
        }
        protected override void OnClose()
        {
            UnsubscribeFromButtons();
        }

        public void ShowButtons()
        {
            int levelCount = _levelService.LevelConfigCount;
            _buttons = new SelectLevelButton[levelCount];
            for (int i = 0; i < levelCount; i++)
            {
                _buttons[i] = Instantiate(_selectLevelButtonPrefab, _buttonContainer);
                _buttons[i].SetLevelIndex(i);
                _buttons[i].SetLevelProgress(_savingService.GetLevelProgress(i));
                _buttons[i].OnClick += HandleClick;
            }
        }

        private void HandleClick(SelectLevelButton button)
        {
            _audioService.PlayOneShot(_clickAudioEvent);

            UnsubscribeFromButtons();
            Debug.Log($"Load level with index {button.LevelIndex}");
        }
        private void UnsubscribeFromButtons()
        {
            if (_buttons == null)
                return;

            int levelCount = _levelService.LevelConfigCount;
            for (int i = 0; i < levelCount; i++)
            {
                _buttons[i].OnClick -= HandleClick;
            }
        }
    }
}
