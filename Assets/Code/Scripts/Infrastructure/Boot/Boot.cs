using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Zenject;
using Core.Infrastructure.Service;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Loading;

namespace Core.Infrastructure.Boot
{
    public class Boot : MonoBehaviour, IInitializable
    {
        [SerializeField] private AssetReference _mainMenuSceneReference;
        [SerializeField] private ClipEvent[] _mainMenuAudio;

        private SceneService _sceneService;

        private ILoadingScreenProvider _loadingScreenProvider;

        [Inject]
        private void Construct(SceneService sceneService, ILoadingScreenProvider loadingScreenProvider)
        {
            _sceneService = sceneService;
            _loadingScreenProvider = loadingScreenProvider;
        }

        public async void Initialize()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;

            await Addressables.InitializeAsync();
            Queue<ILoadingOperation> queue = new Queue<ILoadingOperation>(2);
            queue.Enqueue(new AudioListLoadingOperation(_mainMenuAudio));
            queue.Enqueue(new SceneLoadingOperation(_sceneService, _mainMenuSceneReference));
            await _loadingScreenProvider.LoadAndDestroy(queue);
        }
    }
}
