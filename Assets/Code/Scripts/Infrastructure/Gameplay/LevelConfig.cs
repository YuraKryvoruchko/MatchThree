using UnityEngine;
using Core.Gameplay;

namespace Core.Infrastructure.Gameplay
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "SO/Gameplay/LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        public LevelTask[] Tasks;
    }
}
