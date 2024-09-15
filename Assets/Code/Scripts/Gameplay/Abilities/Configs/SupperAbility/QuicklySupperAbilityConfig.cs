using UnityEngine;

namespace Core.Gameplay
{
    [CreateAssetMenu(fileName = "QuicklySupperAbility", menuName = "SO/Gameplay/Configs/Abilities/SupperAbility/QuicklySupperAbility")]
    public class QuicklySupperAbilityConfig : BaseSupperAbilityConfig
    {
        [Header("Quickly Supper Settings")]
        public int CreatingAbilityObjectNumber;
    }
}
