using UnityEngine;
using Zenject;
using Core.Infrastructure.Service;
using Code.Infrastructure.Loading;
using UnityEngine.AddressableAssets;

namespace Core.Infrastructure.Boot
{
    public class BootstrapInstaller : MonoInstaller
    {
        [SerializeField] private AssetReference _loadingScreenPrefabReference;

        public override void InstallBindings()
        {
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
            BindSceneService();
            BindLoadingScreenProvider();
        }
        
        private void BindSceneService()
        {
            Container
                .Bind<SceneService>()
                .AsSingle();
        }
        private void BindLoadingScreenProvider()
        {
            Container
                .Bind<ILoadingScreenProvider>()
                .To<LoadingScreenProvider>()
                .AsSingle()
                .WithArguments(_loadingScreenPrefabReference);
        }
    }
}
