using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Zenject;
using Core.Infrastructure.Service;

namespace Core.Infrastructure.Boot
{
    public class Boot : MonoBehaviour, IInitializable
    {
        [SerializeField] private AssetReference _mainMenuSceneReference;

        private SceneService _sceneService;

        [Inject]
        private void Construct(SceneService sceneService)
        {
            _sceneService = sceneService;
        }

        public async void Initialize()
        {
            await Addressables.InitializeAsync();
            await _sceneService.LoadSceneAsync(_mainMenuSceneReference.AssetGUID, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
    }
}
