using UnityEngine;
using Zenject;

namespace Core.Infrastructure.Boot
{
    public class BootLevelInstaller : MonoInstaller
    {
        [SerializeField] private Boot _initializableObject;

        public override void InstallBindings()
        {
            BindIInitializableObject();
        }

        private void BindIInitializableObject()
        {
            Container
                .BindInterfacesAndSelfTo<Boot>()
                .FromInstance(_initializableObject)
                .AsSingle();
        }
    }
}
