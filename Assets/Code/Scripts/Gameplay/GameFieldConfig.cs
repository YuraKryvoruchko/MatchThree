using UnityEngine;
using Core.Infrastructure.Service.Audio;

#if UNITY_EDITOR
#endif

namespace Core.Gameplay
{
    [CreateAssetMenu(fileName = "GameFieldConfig", menuName = "SO/Gameplay/GameFieldConfig")]
    public class GameFieldConfig : ScriptableObject
    {
        [Header("Size")]
        [Min(1)] public int VerticalMapSize;
        [Min(2)] public int HorizontalMapSize;
        [Header("Cells")]
        [Min(0)] public float Interval;
        public GameField.BoardCellConfig[] BoardCellConfigs;
        [Header("Gameplay")]
        public CellType[] AvailableRandomCellTypes;
        [Header("Audio Events")]
        public ClipEvent SwipeAudioEvent;
        public ClipEvent CellExplosionAudioEvent;
    }
}
