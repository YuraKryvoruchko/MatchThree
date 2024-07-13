using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.Loading
{
    public class LoadingScreenProvider : LocalAssetLoader, ILoadingScreenProvider
    {
        #region Public Methods

        public async UniTask LoadAndDestroy(ILoadingOperation operation)
        {
            Queue<ILoadingOperation> queue = new Queue<ILoadingOperation>();
            queue.Enqueue(operation);
            await LoadAndDestroy(queue);
        }
        public async UniTask LoadAndDestroy(Queue<ILoadingOperation> operations)
        {
            LoadingScreen loadingScreen = await Load<LoadingScreen>("UI/LoadingScreen");
            await loadingScreen.Load(operations);
            Unload();
        }

        #endregion
    }
}
