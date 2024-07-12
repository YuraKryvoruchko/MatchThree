using UnityEngine;
using Zenject;

namespace Core.Infrastructure.Gameplay
{
    public class GameplayInstaller : MonoInstaller
    {
        [SerializeField] private FieldCellPool _fieldCellPool;
        [SerializeField] private SwipeDetection _swipeDetection;
        [SerializeField] private Camera _mainCamera;

        public override void InstallBindings()
        {
            BindCellFabric();
            BindSwipeDetection();
            BindCellSwipeDetection();
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
        private void BindCellSwipeDetection()
        {
            Container
                .BindInterfacesAndSelfTo<CellSwipeDetection>()
                .AsSingle()
                .WithArguments(_mainCamera);
        }
    }
}
