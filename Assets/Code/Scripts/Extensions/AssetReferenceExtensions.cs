using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Core.Infrastructure.Service
{
    public static class AssetReferenceExtensions
    {
        public static async UniTask<T> GetOrLoad<T>(this AssetReferenceT<T> assetReference, CancellationToken cancellationToken = default)
            where T : UnityEngine.Object
        {
            if (assetReference.Asset == null)
            {
                if (assetReference.IsValid())
                    await assetReference.OperationHandle.Convert<T>().WithCancellation(cancellationToken);
                else
                    await assetReference.LoadAssetAsync().WithCancellation(cancellationToken);
            }

            return assetReference.Asset as T;
        }
    }
}
