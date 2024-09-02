using System;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

using AsyncOperationStatus = UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus;

namespace Core.Infrastructure.Loading
{
    public class ResourcesLoadingOperation : ILoadingOperation
    {
        private AssetReference[] _assetReferences;

        private string _description;

        string ILoadingOperation.Description { get => _description; }

        public ResourcesLoadingOperation(AssetReference[] assetReferences, string description = "Resource Loading...")
        {
            _assetReferences = assetReferences;
            _description = description;
        }

        async UniTask ILoadingOperation.Load(Action<float> onProgress)
        {
            float progress = 0f;
            UniTask[] loadingTasks = new UniTask[_assetReferences.Length];
            for(int i = 0; i < _assetReferences.Length; i++)
            {
                if (_assetReferences[i].IsValid())
                {
                    continue;
                }
                else if (_assetReferences[i].OperationHandle.Status == AsyncOperationStatus.None)
                {
                    loadingTasks[i] = _assetReferences[i].OperationHandle.ToUniTask();
                }
                else
                {
                    loadingTasks[i] = _assetReferences[i].LoadAssetAsync<UnityEngine.Object>().ToUniTask()
                        .ContinueWith((obj) => 
                        {
                            progress += (float)1 / _assetReferences.Length;
                            onProgress?.Invoke(progress);
                        });
                }
            }
            await UniTask.WhenAll(loadingTasks);
        }
    }
}
