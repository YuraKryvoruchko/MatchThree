using Core.Gameplay;
using UnityEngine;
using Zenject;

namespace Core.Infrastructure.Gameplay
{
    public class GameModeInstaller : MonoInstaller
    {
        [Header("Mode")]
        [SerializeField] private bool _isLevelMode;
        [Header("Configs")]
        [SerializeField] private LevelConfig _levelConfig;

        public override void InstallBindings()
        {
            BindPlayerMoveTracking();
            BindLevelTaskCompletionChecker();
            BindGameProgressObserver();
        }
        
        private void BindLevelTaskCompletionChecker()
        {
            if (!_isLevelMode)
                return;

            Container
                .BindInterfacesAndSelfTo<LevelTaskCompletionChecker>()
                .AsSingle()
                .WithArguments(_levelConfig.Tasks);
        }
        private void BindPlayerMoveTracking()
        {
            Container
                .BindInterfacesAndSelfTo<PlayerMoveTracking>()
                .AsSingle();
        }
        private void BindGameProgressObserver()
        {
            if (_isLevelMode)
            {
                Container
                    .BindInterfacesTo<LevelModeSimulation>()
                    .AsSingle();
            }
            else
            {
                Container
                    .BindInterfacesTo<LongModeSimulation>()
                    .AsSingle();
            }
        }
    }
}
