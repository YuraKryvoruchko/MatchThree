using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using Zenject;
using Cysharp.Threading.Tasks;
using Core.Infrastructure.Service;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Loading;
using Core.Infrastructure.UI;
using Core.Infrastructure.Service.Saving;
using Core.Infrastructure.Gameplay;

namespace Core.UI.Menu
{
    public class MainMenu : WindowBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _chooseLevelButton;
        [SerializeField] private Button _startLongModeButton;
        [SerializeField] private Button _settingsButton;
        [Header("Scene Keys")]
        [SerializeField] private AssetReference _mainMenuScene;
        [SerializeField] private AssetReference _gamePlayScene;
        [Header("Select Level Menu")]
        [SerializeField] private AssetReferenceGameObject _selectLevelMenuReference;
        [SerializeField] private AssetReferenceGameObject _settingPopupReference;
        [Header("Long Mode Settings")]
        [SerializeField] private LevelConfig _longModeLevelConfig;
        [Header("Audio Keys")]
        [SerializeField] private ClipEvent _clickAudioPath;
        [Header("Loadable Audio")]
        [SerializeField] private ClipEvent[] _unloadingAudioList;
        [SerializeField] private ClipEvent[] _loadingAudioList;

        private ILoadingScreenProvider _loadingScreenProvider;

        private SceneService _sceneService;
        private IWindowService _windowService;
        private IAudioService _audioService;
        private ILevelService _levelService;

        public override event Action OnMenuBack;

        [Inject]
        private void Construct(SceneService sceneService, IWindowService windowService, IAudioService audioService, 
            ILoadingScreenProvider loadingScreenProvider, ILevelService levelService)
        {
            _windowService = windowService;
            _sceneService = sceneService;
            _audioService = audioService;
            _loadingScreenProvider = loadingScreenProvider;
            _levelService = levelService;
        }

        protected override void OnShow()
        {
            _startLongModeButton.onClick.AddListener(() => _audioService.PlayOneShot(_clickAudioPath));
            _startLongModeButton.onClick.AddListener(LoadLongMode);
            _chooseLevelButton.onClick.AddListener(() => _audioService.PlayOneShot(_clickAudioPath));
            _chooseLevelButton.onClick.AddListener(OpenSelectLevelMenu);
            _settingsButton.onClick.AddListener(() => _audioService.PlayOneShot(_clickAudioPath));
            _settingsButton.onClick.AddListener(OpenSettingPopup);
            gameObject.SetActive(true);
        }
        protected override void OnHide()
        {
            _startLongModeButton.onClick.RemoveAllListeners();
            _chooseLevelButton.onClick.RemoveAllListeners();
            _settingsButton.onClick.RemoveAllListeners();
            gameObject.SetActive(false);
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

        private void LoadLongMode()
        {
            PlayerPrefs.SetInt(PlayerPrefsEnum.GameModeSettings.IS_LEVEL_MODE_VALUE, 0);
            _levelService.SetCustomLevelConfig(_longModeLevelConfig);
            Queue<ILoadingOperation> queue = new Queue<ILoadingOperation>(4);
            queue.Enqueue(new AudioListUnloadingOperation(_unloadingAudioList));
            queue.Enqueue(new SceneUnloadingOperation(_sceneService, _mainMenuScene));
            queue.Enqueue(new AudioListLoadingOperation(_loadingAudioList));
            queue.Enqueue(new SceneLoadingOperation(_sceneService, _gamePlayScene));
            _loadingScreenProvider.LoadAndDestroy(queue);
        }
        private void OpenSelectLevelMenu()
        {
            UniTask.Void(async () =>
            {
                SelectLevelMenu selectLevelMenu = await _windowService.OpenWindow<SelectLevelMenu>(_selectLevelMenuReference.AssetGUID);
                selectLevelMenu.transform.SetParent(transform.parent);
                selectLevelMenu.ShowButtons();
            });
        }
        private void OpenSettingPopup()
        {
            UniTask.Void(async () =>
            {
                SettingPopup selectLevelMenu = await _windowService.OpenPopup<SettingPopup>(_settingPopupReference.AssetGUID);
                selectLevelMenu.transform.SetParent(transform.parent);
            });
        }
        private void SetInteractable(bool interactable)
        {
            _chooseLevelButton.interactable = interactable;
            _startLongModeButton.interactable = interactable;
            _settingsButton.interactable = interactable;
        }
    }
}
