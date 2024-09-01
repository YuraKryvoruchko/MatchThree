using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;
using Core.UI;

namespace Core.Infrastructure.MainMenu
{
    public class MainMenuInstaller : MonoInstaller
    {
        [Header("UI Startup Settings")]
        [SerializeField] private AssetReferenceGameObject _mainMenuReference;
        [SerializeField] private Transform _uiContainer;

        public override void InstallBindings()
        {
            BindUIStartup();
        }

        private void BindUIStartup()
        {
            Container
                .BindInterfacesTo<UIStartup>()
                .AsSingle()
                .WithArguments(_mainMenuReference, _uiContainer);
        }
    }
}
