using UnityEngine;

namespace Core.Gameplay
{
    [CreateAssetMenu(fileName = "CellConfig", menuName = "SO/Gameplay/CellConfig", order = 1)]
    public class CellConfig : ScriptableObject
    {
        [Header("Visual")]
        public Sprite Icon;
        [Header("Type")]
        public CellType Type;
        [Header("Features")]
        public bool IsStatic = false;
        public bool IsSpecial = false;
    }
}