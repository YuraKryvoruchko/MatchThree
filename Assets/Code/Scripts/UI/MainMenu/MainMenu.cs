using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using Zenject;
using Cysharp.Threading.Tasks;
using Core.Infrastructure.Service;

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

        private SceneService _sceneService;

        [Inject]
        private void Construct(SceneService sceneService)
        {
            _sceneService = sceneService;
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
            var gamePlayScene = await _sceneService.LoadSceneAsync(_gamePlayScene.AssetGUID, UnityEngine.SceneManagement.LoadSceneMode.Additive, false);
            await _sceneService.UnloadSceneAsync(_mainMenuScene.AssetGUID);
            await gamePlayScene.ActivateAsync();
        }
    }
}
