using UnityEngine;
using UnityEngine.AddressableAssets;
using Core.Infrastructure.Service.Audio;

namespace Core.Gameplay
{
    [CreateAssetMenu(fileName = "BaseSupperAbilityConfig", menuName = "SO/Gameplay/Configs/Abilities/SupperAbility/BaseSupperAbilityConfig")]
    public class BaseSupperAbilityConfig : ScriptableObject
    {
        [Header("VFX")]
        public AssetReferenceGameObject VfxPrefab;
        [Header("Audio")]
        public ClipEvent ElementCapturingEvent;
        public ClipEvent ElementExplosionEvent;
    }
}
