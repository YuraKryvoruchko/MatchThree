using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;
using Core.Infrastructure.Service;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Loading;
using Core.Infrastructure.Service.Saving;
using Core.Infrastructure.Gameplay;

namespace Core.Infrastructure.Boot
{
    public class BootstrapInstaller : MonoInstaller
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioServiceConfig _audioServiceConfig;
        [Header("Level Settings")]
        [SerializeField] private LevelConfigContainer _levelConfigContainer;
        [Header("Loading Screen")]
        [SerializeField] private AssetReference _loadingScreenPrefabReference;

        public override void InstallBindings()
        {
            BindAudioService();
            BindSceneService();
            BindSavingService();
            BindLevelService();
            BindLoadingScreenProvider();
        }
        
        private void BindAudioService()
        {
            Container
                .BindInterfacesAndSelfTo<AudioService>()
                .AsSingle()
                .WithArguments(_audioServiceConfig);
            Container
                .BindInterfacesTo<SetupAudioService>()
                .AsSingle();
        }
        private void BindSceneService()
        {
            Container
                .Bind<SceneService>()
                .AsSingle();
        }
        private void BindSavingService()
        {
            Container
                .BindInterfacesAndSelfTo<SavingService>()
                .AsSingle();
        }
        private void BindLevelService()
        {
            Container
                .Bind<ILevelService>()
                .To<LevelService>()
                .AsSingle()
                .WithArguments(_levelConfigContainer);
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
