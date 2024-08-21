using UnityEngine;
using Zenject;
using Core.Infrastructure.Service;
using Core.Infrastructure.UI;
using Cysharp.Threading.Tasks;

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

        void IInitializable.Initialize()
        {
            UniTask.Void(async () => 
            {
                GameplayMenu menu = await _windowService.OpenWindow<GameplayMenu>(_gameplayMenuPrefab.Path);
                menu.transform.SetParent(_uiContainer);
            });
        }
    }
}
