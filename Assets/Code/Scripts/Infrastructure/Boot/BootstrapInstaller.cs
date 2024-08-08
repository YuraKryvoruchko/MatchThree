using UnityEngine;
using Zenject;
using Core.Infrastructure.Service;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Loading;
using UnityEngine.AddressableAssets;
using System;

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
