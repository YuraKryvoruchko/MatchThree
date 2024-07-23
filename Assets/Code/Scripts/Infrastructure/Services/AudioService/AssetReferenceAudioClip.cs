using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Infrastructure.Service
{
    [System.Serializable]
    public class AssetReferenceAudioClip : AssetReferenceT<AudioClip>
    {
        public AssetReferenceAudioClip(string guid) : base(guid)
        {
        }
    }
}
