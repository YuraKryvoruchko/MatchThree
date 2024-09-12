using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

namespace Core.Infrastructure.Service
{
    public static class AssetReferenceExtensions
    {
        public static async UniTask<T> GetOrLoad<T>(this AssetReferenceT<T> assetReference) where T : UnityEngine.Object
        {
            if (assetReference.Asset == null)
            {
                if (assetReference.IsValid())
                    await assetReference.OperationHandle.Convert<T>();
                else
                    await assetReference.LoadAssetAsync();
            }

            return assetReference.Asset as T;
        }
    }
}
