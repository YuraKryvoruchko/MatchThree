using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Gameplay
{
    [CreateAssetMenu(fileName = "CellConfig", menuName = "SO/Gameplay/CellConfig", order = 1)]
    public class CellConfig : ScriptableObject
    {
        [Header("Visual")]
        public AssetReferenceSprite Icon;
        [Header("Settings")]
        public CellType Type;
        public int Score;
        [Header("Features")]
        public bool IsStatic = false;
        public bool IsSpecial = false;
        public bool IsSpawn = false;
    }
}