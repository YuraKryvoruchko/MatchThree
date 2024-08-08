using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;
using Core.Infrastructure.Service;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Service.Pause;
using Core.Infrastructure.Loading;

namespace Core.Infrastructure.Boot
{
    public class BootstrapInstaller : MonoInstaller
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioServiceConfig _audioServiceConfig;
        [Header("Loading Screen")]
        [SerializeField] private AssetReference _loadingScreenPrefabReference;

        public override void InstallBindings()
        {
            BindAudioService();
            BindSceneService();
            BindPauseServiceAndPauseProvider();
            BindLoadingScreenProvider();
        }
        
        private void BindAudioService()
        {
            Container
                .BindInterfacesAndSelfTo<AudioService>()
                .AsSingle()
                .WithArguments(_audioServiceConfig);
        }
        private void BindSceneService()
        {
            Container
                .Bind<SceneService>()
                .AsSingle();
        }
        private void BindPauseServiceAndPauseProvider()
        {
            Container
                .Bind(typeof(IPauseService), typeof(IPauseProvider))
                .To<PauseService>()
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
