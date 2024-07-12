using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Core.Infrastructure.Boot
{
    public class Boot : MonoBehaviour, IInitializable
    {
        [SerializeField] private AssetReference _mainMenuSceneReference;

        public async void Initialize()
        {
            await Addressables.InitializeAsync();
            await Addressables.LoadSceneAsync(_mainMenuSceneReference, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
    }
}
