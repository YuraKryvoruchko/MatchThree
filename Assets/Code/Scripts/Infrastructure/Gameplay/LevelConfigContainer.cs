using UnityEngine;

namespace Core.Infrastructure.Gameplay
{
    [CreateAssetMenu(fileName = "LevelConfigContainer", menuName = "SO/Gameplay/LevelConfigContainer")]
    public class LevelConfigContainer : ScriptableObject
    {
        public LevelConfig[] LevelConfigs;
    }
}
