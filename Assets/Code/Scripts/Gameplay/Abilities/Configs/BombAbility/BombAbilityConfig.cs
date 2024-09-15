using UnityEngine;
using UnityEngine.AddressableAssets;
using Core.Infrastructure.Service.Audio;

namespace Core.Gameplay
{
    [CreateAssetMenu(fileName = "BombAbilityConfig", menuName = "SO/Gameplay/Configs/Abilities/BombAbilityConfig")]
    public class BombAbilityConfig : ScriptableObject
    {
        [Header("Settings")]
        public int LineLength;
        [Header("VFX")]
        public AssetReferenceGameObject VfxPrefab;
        [Header("Audio")]
        public ClipEvent ExplosiveEvent;
    }
}
