using UnityEngine;
using Core.Gameplay;
using Core.UI.Gameplay;

namespace Core.Infrastructure.Gameplay
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "SO/Gameplay/LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Config")]
        public int MoveCount;
        [Header("Tasks")]
        public LevelTask[] Tasks;
        [Header("Abilities In Holder")]
        public HolderAbilitySettings[] AbilitySettings;
        [Header("Game Field Config")]
        public GameFieldConfig GameFieldConfig;

        public const int ACCUMULATION_MODE = -1;
    }
}
