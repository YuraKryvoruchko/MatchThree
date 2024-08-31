using UnityEngine;
using Core.Infrastructure.Gameplay;

namespace Core.Infrastructure.Service
{
    public class LevelService : ILevelService
    {
        private LevelConfigContainer _levelConfigContainer;

        private const int RESET_INDEX_VALUE = -1;

        public int LevelConfigCount { get => _levelConfigContainer.LevelConfigs.Length; }
        public int CurentLevelConfigIndex { get; private set; }

        public LevelService(LevelConfigContainer levelConfigContainer)
        {
            _levelConfigContainer = levelConfigContainer;

            ResetLevelConfigIndex();
        }

        public bool IsLevelConfigSeted()
        {
            return CurentLevelConfigIndex != RESET_INDEX_VALUE;
        }
        public void SetCurrentLevelConfigByIndex(int index)
        {
            CurentLevelConfigIndex = Mathf.Clamp(index, 0, LevelConfigCount);
        }
        public void ResetLevelConfigIndex()
        {
            CurentLevelConfigIndex = RESET_INDEX_VALUE;
        }
        public LevelConfig GetCurrentLevelConfig()
        {
            return GetLevelConfigByIndex(CurentLevelConfigIndex);
        }
        public LevelConfig GetLevelConfigByIndex(int index)
        {
            return _levelConfigContainer.LevelConfigs[Mathf.Clamp(index, 0, LevelConfigCount)];
        }
    }
}
