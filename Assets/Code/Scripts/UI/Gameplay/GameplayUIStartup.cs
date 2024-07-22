using UnityEngine;
using Zenject;
using Core.Infrastructure.Service;
using Core.Infrastructure.UI;

namespace Core.UI.Gameplay
{
    public class GameplayUIStartup : MonoBehaviour, IInitializable
    {
        [SerializeField] private WindowBase _gameplayMenuPrefab;
        [SerializeField] private Transform _uiContainer;

        private IWindowService _windowService;

        [Inject]
        private void Construct(IWindowService windowService)
        {
            _windowService = windowService;
        }

        async void IInitializable.Initialize()
        {
            GameplayMenu menu = await _windowService.OpenWindow<GameplayMenu>(_gameplayMenuPrefab.Path);
            menu.transform.parent = _uiContainer;
        }
    }
}
