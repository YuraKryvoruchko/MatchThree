using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;
using Cysharp.Threading.Tasks;
using Core.Infrastructure.Service;
using Core.Infrastructure.UI;

namespace Core.UI
{
    public class UIStartup : IInitializable
    {
        private AssetReferenceGameObject _gameplayMenuReference;
        private Transform _uiContainer;

        private IWindowService _windowService;

        public UIStartup(IWindowService windowService, AssetReferenceGameObject gameplayMenuReference, Transform uiContainer)
        {
            _windowService = windowService;
            _gameplayMenuReference = gameplayMenuReference;
            _uiContainer = uiContainer;
        }

        void IInitializable.Initialize()
        {
            UniTask.Void(async () => 
            {
                WindowBase menu = await _windowService.OpenWindow<WindowBase>(_gameplayMenuReference.AssetGUID);
                menu.transform.SetParent(_uiContainer);
            });
        }
    }
}
