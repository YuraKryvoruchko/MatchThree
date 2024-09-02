using System;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

namespace Core.Infrastructure.Loading
{
    public class ResourcesUnloadingOperation : ILoadingOperation
    {
        private AssetReference[] _assetReferences;

        private string _description;

        string ILoadingOperation.Description { get => _description; }

        public ResourcesUnloadingOperation(AssetReference[] assetReferences, string description = "Resource Unloading...")
        {
            _assetReferences = assetReferences;
            _description = description;
        }

        UniTask ILoadingOperation.Load(Action<float> onProgress)
        {
            for (int i = 0; i < _assetReferences.Length; i++)
            {
                _assetReferences[i].ReleaseAsset();
            }

            return UniTask.FromResult(0);
        }
    }
}
