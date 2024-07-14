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
        [SerializeField] private FieldCellPool _fieldCellPool;
        [SerializeField] private SwipeDetection _swipeDetection;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private GameField _gameField;

        public override void InstallBindings()
        {
            BindCellFabric();
            BindSwipeDetection();
            BindCellClickDetection();
            BindCellSwipeDetection();
            BindGameField();
            BindAbilityThrowMode();
        }

        private void BindCellFabric()
        {
            Container
                .Bind<ICellFabric>()
                .FromInstance(_fieldCellPool)
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
    }
}
