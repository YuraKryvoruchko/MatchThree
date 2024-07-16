using UnityEngine;
using Zenject;
using Core.Input;
using Core.Gameplay.Input;
using Core.Infrastructure.Factories;
using Core.Gameplay;

namespace Core.Infrastructure.Gameplay
{
    public class GameplayInstaller : MonoInstaller
    {
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private GameField _gameField;
        [SerializeField] private SwipeDetection _swipeDetection;
        [SerializeField] private FieldCellFabric.FieldCellFabricConfig _cellFabricConfig;

        public override void InstallBindings()
        {
            BindCellFabric();
            BindAbilityFabric();
            BindSwipeDetection();
            BindCellClickDetection();
            BindCellSwipeDetection();
            BindGameField();
            BindAbilityThrowMode();
            BindGameScoreTracking();
        }

        private void BindCellFabric()
        {
            Container
                .Bind<ICellFabric>()
                .To<FieldCellFabric>()
                .AsSingle()
                .WithArguments(_cellFabricConfig);
        }
        private void BindAbilityFabric()
        {
            Container
                .Bind<IAbilityFactory>()
                .To<AbilityFactory>()
                .AsSingle();
        }
        private void BindSwipeDetection()
        {
            Container
                .Bind<SwipeDetection>()
                .FromInstance(_swipeDetection)
                .AsSingle();
        }
        private void BindCellClickDetection()
        {
            Container
                .BindInterfacesAndSelfTo<CellClickDetection>()
                .AsSingle()
                .WithArguments(_mainCamera);
        }
        private void BindCellSwipeDetection()
        {
            Container
                .BindInterfacesAndSelfTo<CellSwipeDetection>()
                .AsSingle()
                .WithArguments(_mainCamera);
        }
        private void BindGameField()
        {
            Container
                .Bind<GameField>()
                .FromInstance(_gameField)
                .AsSingle();
        }
        private void BindAbilityThrowMode()
        {
            Container
                .Bind<AbilityThrowMode>()
                .AsSingle();
        }
        private void BindGameScoreTracking()
        {
            Container
                .BindInterfacesAndSelfTo<GameScoreTracking>()
                .AsSingle();
        }
    }
}
