<<<<<<< HEAD
﻿using System;
using UnityEngine;
using UnityEngine.UI;
=======
﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
>>>>>>> b34025bea1305964adb62870fd56e19165d73720
using Zenject;
using Core.Infrastructure.Service;
using Core.Infrastructure.UI;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Loading;
<<<<<<< HEAD
using Core.Infrastructure.Service.Saving;
=======
using System;
>>>>>>> b34025bea1305964adb62870fd56e19165d73720

namespace Core.UI
{
    public class SelectLevelMenu : WindowBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _backButton;
<<<<<<< HEAD
        [Header("Select Button Settings")]
        [SerializeField] private SelectLevelButton _selectLevelButtonPrefab;
        [SerializeField] private Transform _buttonContainer;
        [Header("Audio Keys")]
        [SerializeField] private ClipEvent _clickAudioEvent;

        private SelectLevelButton[] _buttons;
=======
        [Header("Scene Keys")]
        [SerializeField] private AssetReferenceGameObject _levelSelectionButtonReference;
        [Header("Audio Keys")]
        [SerializeField] private ClipEvent _clickAudioPath;
>>>>>>> b34025bea1305964adb62870fd56e19165d73720

        private ILoadingScreenProvider _loadingScreenProvider;

        private SceneService _sceneService;
        private IAudioService _audioService;
        private ILevelService _levelService;
<<<<<<< HEAD
        private ISavingService _savingService;
=======
>>>>>>> b34025bea1305964adb62870fd56e19165d73720

        public override event Action OnMenuBack;

        [Inject]
<<<<<<< HEAD
        private void Construct(SceneService sceneService, IAudioService audioService, ILevelService levelService, ISavingService savingService,
=======
        private void Construct(SceneService sceneService, IAudioService audioService, ILevelService levelService, 
>>>>>>> b34025bea1305964adb62870fd56e19165d73720
            ILoadingScreenProvider loadingScreenProvider)
        {
            _sceneService = sceneService;
            _audioService = audioService;
            _levelService = levelService;
<<<<<<< HEAD
            _savingService = savingService;
=======
>>>>>>> b34025bea1305964adb62870fd56e19165d73720
            _loadingScreenProvider = loadingScreenProvider;
        }

        protected override void OnShow()
        {
<<<<<<< HEAD
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
=======
            throw new NotImplementedException();
        }
        protected override void OnHide()
        {
            throw new NotImplementedException();
        }
        protected override void OnFocus()
        {
            throw new NotImplementedException();
        }
        protected override void OnUnfocus()
        {
            throw new NotImplementedException();
        }
        protected override void OnClose()
        {
            throw new NotImplementedException();
>>>>>>> b34025bea1305964adb62870fd56e19165d73720
        }
    }
}
