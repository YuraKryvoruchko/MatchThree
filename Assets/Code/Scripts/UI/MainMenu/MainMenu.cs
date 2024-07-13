using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using Zenject;
using Cysharp.Threading.Tasks;
using Core.Infrastructure.Service;
using Code.Infrastructure.Loading;
using System;

namespace Core.UI
{
    public class MainMenu : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _chooseLevelButton;
        [SerializeField] private Button _startLongModeButton;
        [SerializeField] private Button _settingsButton;
        [Header("Scene Keys")]
        [SerializeField] private AssetReference _mainMenuScene;
        [SerializeField] private AssetReference _gamePlayScene;

        private ILoadingScreenProvider _loadingScreenProvider;

        private SceneService _sceneService;

        private class GameplaySceneLoadingOperation : ILoadingOperation
        {
            private AssetReference _gamePlayScene;
            private AssetReference _mainMenuScene;

            private SceneService _sceneService;

            string ILoadingOperation.Description => "Loading level...";

            public GameplaySceneLoadingOperation(SceneService sceneService, AssetReference gamePlayScene, AssetReference mainMenuScene)
            {
                _sceneService = sceneService;
                _gamePlayScene = gamePlayScene;
                _mainMenuScene = mainMenuScene;
            }

            async UniTask ILoadingOperation.Load(Action<float> onProgress)
            {
                await _sceneService.UnloadSceneAsync(_mainMenuScene.AssetGUID);
                onProgress?.Invoke(0.6f);

                var gamePlayScene = await _sceneService.LoadSceneAsync(_gamePlayScene.AssetGUID,
                    UnityEngine.SceneManagement.LoadSceneMode.Additive);
                onProgress?.Invoke(1f);
            }
        }

        [Inject]
        private void Construct(SceneService sceneService, ILoadingScreenProvider loadingScreenProvider)
        {
            _sceneService = sceneService;
            _loadingScreenProvider = loadingScreenProvider;
        }

        private void Awake()
        {
            _startLongModeButton.onClick.AddListener(LoadLongMode);
        }
        private void OnDestroy()
        {
            _startLongModeButton.onClick.RemoveAllListeners();
        }

        private async void LoadLongMode()
        {
            await _loadingScreenProvider.LoadAndDestroy(
                new GameplaySceneLoadingOperation(_sceneService, _gamePlayScene, _mainMenuScene));
        }
    }
}
