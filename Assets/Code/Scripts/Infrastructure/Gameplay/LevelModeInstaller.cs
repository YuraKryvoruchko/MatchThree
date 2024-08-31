using Core.Gameplay;
using UnityEngine;
using Zenject;

namespace Core.Infrastructure.Gameplay
{
    public class LevelModeInstaller : MonoInstaller
    {
        [SerializeField] private LevelConfig _levelConfig;

        public override void InstallBindings()
        {
            BindLevelTaskCompletionChecker();
            BindPlayerMoveObserver();
            BindGameProgressObserver();
        }
        
        private void BindLevelTaskCompletionChecker()
        {
            Container
                .BindInterfacesAndSelfTo<LevelTaskCompletionChecker>()
                .AsSingle()
                .WithArguments(_levelConfig.Tasks);
        }
        private void BindPlayerMoveObserver()
        {
            Container
                .BindInterfacesAndSelfTo<PlayerMoveObserver>()
                .AsSingle();
        }
        private void BindGameProgressObserver()
        {
            Container
                .BindInterfacesAndSelfTo<GameProgressObserver>()
                .AsSingle();
        }
    }
}
