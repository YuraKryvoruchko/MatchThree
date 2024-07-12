using UnityEngine;
using Zenject;

namespace Core.Infrastructure
{
    public class BootstrapInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
        }
    }
}
