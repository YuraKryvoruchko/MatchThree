using System;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Core.Infrastructure.Service;

namespace Core.Infrastructure.Loading
{
    public class SceneLoadingOperation : ILoadingOperation
    {
        private AssetReference _sceneReference;

        private SceneService _sceneService;

        string ILoadingOperation.Description => "Loading the scene...";

        public SceneLoadingOperation(SceneService sceneService, AssetReference sceneReference)
        {
            _sceneService = sceneService;
            _sceneReference = sceneReference;
        }

        async UniTask ILoadingOperation.Load(Action<float> onProgress)
        {
            onProgress?.Invoke(0.3f);
            var gamePlayScene = await _sceneService.LoadSceneAsync(_sceneReference.AssetGUID,
                UnityEngine.SceneManagement.LoadSceneMode.Additive);
            onProgress?.Invoke(1f);
        }
    }
}
