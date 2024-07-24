using UnityEngine;
using Zenject;
using Core.Infrastructure.Service;
using Code.Infrastructure.Loading;
using UnityEngine.AddressableAssets;

namespace Core.Infrastructure.Boot
{
    public class BootstrapInstaller : MonoInstaller
    {
        [Header("Audio Settings")]
        [SerializeField] private BackgroundAudioServiceConfig _backgroundAudioServiceConfig;
        [SerializeField] private UIAudioServiceConfig _UIAudioServiceConfig;
        [Header("Loading Screen")]
        [SerializeField] private AssetReference _loadingScreenPrefabReference;

        public override void InstallBindings()
        {
            BindBackgroundAudioService();
            BindUIAudioService();
            BindSceneService();
            BindLoadingScreenProvider();
        }
        
        private void BindBackgroundAudioService()
        {
            Container
                .BindInterfacesAndSelfTo<BackgroundAudioService>()
                .AsSingle()
                .WithArguments(_backgroundAudioServiceConfig);
        }
        private void BindUIAudioService()
        {
            Container
                .BindInterfacesAndSelfTo<UIAudioService>()
                .AsSingle()
                .WithArguments(_UIAudioServiceConfig);
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
