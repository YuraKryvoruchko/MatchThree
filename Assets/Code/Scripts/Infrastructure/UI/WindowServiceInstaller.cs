using Zenject;
using Core.Infrastructure.Factories;
using Core.Infrastructure.Service;

namespace Core.Infrastructure.UI
{
    public class WindowServiceInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindWindowFactory();
            BindWindowService();
        }

        private void BindWindowFactory()
        {
            Container
                .Bind<IWindowFactory>()
                .To<WindowFactory>()
                .AsSingle();
        }
        private void BindWindowService()
        {
            Container
                .Bind<IWindowService>()
                .To<WindowService>()
                .AsSingle();
        }
    }
}
