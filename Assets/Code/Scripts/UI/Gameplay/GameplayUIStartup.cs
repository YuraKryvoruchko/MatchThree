using UnityEngine;
using Zenject;
using Core.Infrastructure.Service;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Core.UI.Gameplay
{
    public class GameplayUIStartup : IInitializable
    {
        private AssetReferenceGameObject _gameplayMenuReference;
        private Transform _uiContainer;

        private IWindowService _windowService;

        public GameplayUIStartup(IWindowService windowService, AssetReferenceGameObject gameplayMenuReference, Transform uiContainer)
        {
            _windowService = windowService;
            _gameplayMenuReference = gameplayMenuReference;
            _uiContainer = uiContainer;
        }

        void IInitializable.Initialize()
        {
            UniTask.Void(async () => 
            {
                GameplayMenu menu = await _windowService.OpenWindow<GameplayMenu>(_gameplayMenuReference.AssetGUID);
                menu.transform.SetParent(_uiContainer);
            });
        }
    }
}
