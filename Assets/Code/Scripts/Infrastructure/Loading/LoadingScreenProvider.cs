using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Code.Infrastructure.Loading
{
    public class LoadingScreenProvider : LocalAssetLoader, ILoadingScreenProvider
    {
        private readonly AssetReference _loadingScreenPrefabReference;

        public LoadingScreenProvider(AssetReference loadingScreenPrefabReference)
        {
            _loadingScreenPrefabReference = loadingScreenPrefabReference;
        }

        public async UniTask LoadAndDestroy(ILoadingOperation operation)
        {
            Queue<ILoadingOperation> queue = new Queue<ILoadingOperation>();
            queue.Enqueue(operation);
            await LoadAndDestroy(queue);
        }
        public async UniTask LoadAndDestroy(Queue<ILoadingOperation> operations)
        {
            LoadingScreen loadingScreen = await Load<LoadingScreen>(_loadingScreenPrefabReference.AssetGUID);
            await loadingScreen.Load(operations);
            Unload();
        }
    }
}
