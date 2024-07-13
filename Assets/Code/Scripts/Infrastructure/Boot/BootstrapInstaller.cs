using UnityEngine;
using Zenject;
using Core.Infrastructure.Service;

namespace Core.Infrastructure.Boot
{
    public class BootstrapInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
            BindSceneService();
        }
        
        private void BindSceneService()
        {
            Container
                .Bind<SceneService>()
                .AsSingle();
        }
    }
}
