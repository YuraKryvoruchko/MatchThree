using UnityEngine;
using UnityEngine.AddressableAssets;
using Core.Infrastructure.Service.Audio;

namespace Core.Gameplay
{
    [CreateAssetMenu(fileName = "LightningBoltAbilityConfig", menuName = "SO/Gameplay/Configs/Abilities/LightningBoltAbilityConfig")]
    public class LightningBoltAbilityConfig : ScriptableObject
    {
        [Header("Settings")]
        public int MaxLightningBoltCount;
        [Header("VFX")]
        public AssetReferenceGameObject VFXPrefab;
        [Header("Audio")]
        public ClipEvent HitEvent;
    }
}
