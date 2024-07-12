using UnityEngine;
using Zenject;

namespace Core.Infrastructure.Gameplay
{
    public class GameplayInstaller : MonoInstaller
    {
        [SerializeField] private SwipeDetection _swipeDetection;
        [SerializeField] private Camera _mainCamera;

        public override void InstallBindings()
        {
            BindSwipeDetection();
            BindCellSwipeDetection();
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
