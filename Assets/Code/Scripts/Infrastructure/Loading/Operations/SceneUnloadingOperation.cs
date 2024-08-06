using System;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Core.Infrastructure.Service;

namespace Core.Infrastructure.Loading
{
    public class SceneUnloadingOperation : ILoadingOperation
    {
        private AssetReference _sceneReference;

        private SceneService _sceneService;

        string ILoadingOperation.Description => "Unloading the scene...";

        public SceneUnloadingOperation(SceneService sceneService, AssetReference sceneReference)
        {
            _sceneService = sceneService;
            _sceneReference = sceneReference;
        }

        async UniTask ILoadingOperation.Load(Action<float> onProgress)
        {
            onProgress?.Invoke(0.3f);
            await _sceneService.UnloadSceneAsync(_sceneReference.AssetGUID);
            onProgress?.Invoke(1f);
        }
    }
}
