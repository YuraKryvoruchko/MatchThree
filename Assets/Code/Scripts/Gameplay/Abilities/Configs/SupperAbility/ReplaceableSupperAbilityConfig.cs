using UnityEngine;

namespace Core.Gameplay
{
    [CreateAssetMenu(fileName = "ReplaceableSupperAbilityConfig", menuName = "SO/Gameplay/Configs/Abilities/SupperAbility/ReplaceableSupperAbilityConfig")]
    public class ReplaceableSupperAbilityConfig : BaseSupperAbilityConfig
    {
        [Header("Replaceable Settings")]
        public int CreatingAbilityObjectNumber;
        public CellType ReplaceObjectType;
    }
}
