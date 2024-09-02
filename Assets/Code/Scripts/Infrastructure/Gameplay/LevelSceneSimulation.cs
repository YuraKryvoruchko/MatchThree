using System.Collections.Generic;
using Core.Infrastructure.Service;
using UnityEngine.AddressableAssets;
using Core.Infrastructure.Loading;

namespace Core.Infrastructure.Gameplay
{
    public class LevelSceneSimulation : ILevelSceneSimulation
    {
        private SceneService _sceneService;

        private AssetReference _gameScene;
        private AssetReference _mainMenuScene;

        private ILoadingScreenProvider _loadingScreenProvider;

        public LevelSceneSimulation(SceneService sceneService, AssetReference gameScene, AssetReference mainMenuScene,
            ILoadingScreenProvider loadingScreenProvider)
        {
            _sceneService = sceneService;
            _gameScene = gameScene;
            _mainMenuScene = mainMenuScene;
            _loadingScreenProvider = loadingScreenProvider;
        }

        public void QuitToMainMenu()
        {
            Queue<ILoadingOperation> operations = new Queue<ILoadingOperation>(2);
            operations.Enqueue(new SceneUnloadingOperation(_sceneService, _gameScene));
            operations.Enqueue(new SceneLoadingOperation(_sceneService, _mainMenuScene));
            _loadingScreenProvider.LoadAndDestroy(operations);
        }
        public void RestartLevel()
        {
            Queue<ILoadingOperation> operations = new Queue<ILoadingOperation>(2);
            operations.Enqueue(new SceneUnloadingOperation(_sceneService, _gameScene));
            operations.Enqueue(new SceneLoadingOperation(_sceneService, _gameScene));
            _loadingScreenProvider.LoadAndDestroy(operations);
        }
    }
}
