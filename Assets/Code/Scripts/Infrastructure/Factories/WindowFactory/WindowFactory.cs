using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;
using Cysharp.Threading.Tasks;
using Core.Infrastructure.UI;

namespace Core.Infrastructure.Factories
{
    public class WindowFactory : IWindowFactory
    {
        #region Fields

        private DiContainer _diContainer;

        #endregion

        #region Constructor

        public WindowFactory(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }

        #endregion

        #region Public Methods

        public async UniTask<T> GetWindow<T>(string key, Transform parent = null) where T : WindowBase
        {
            GameObject windowAsset = await Addressables.LoadAssetAsync<GameObject>(key);
            return _diContainer.InstantiatePrefabForComponent<T>(windowAsset);
        }
        public void ReleaseWindow(WindowBase window)
        {
            Object.Destroy(window.gameObject);
        }

        #endregion
    }
}
