using UnityEngine;
using Core.Infrastructure.Gameplay;

namespace Core.Infrastructure.Service
{
    public class LevelService : ILevelService
    {
        private LevelConfigContainer _levelConfigContainer;

        private LevelConfig _customLevelConfig;

        private const int RESET_INDEX_VALUE = -1;

        public int LevelConfigCount { get => _levelConfigContainer.LevelConfigs.Length; }
        public int CurentLevelConfigIndex { get; private set; }

        public LevelService(LevelConfigContainer levelConfigContainer)
        {
            _levelConfigContainer = levelConfigContainer;

            ResetLevelConfig();
        }

        public bool IsLevelConfigSeted()
        {
            return CurentLevelConfigIndex != RESET_INDEX_VALUE || _customLevelConfig != null;
        }
        public bool IsLevelConfigCustom()
        {
            return _customLevelConfig != null;
        }
        public void SetCurrentLevelConfigByIndex(int index)
        {
            CurentLevelConfigIndex = Mathf.Clamp(index, 0, LevelConfigCount);
        }
        public void ResetLevelConfig()
        {
            CurentLevelConfigIndex = RESET_INDEX_VALUE;
            _customLevelConfig = null;
        }
        public void SetCustomLevelConfig(LevelConfig config)
        {
            _customLevelConfig = config;
        }
        public LevelConfig GetCurrentLevelConfig()
        {
            if(IsLevelConfigCustom())
                return _customLevelConfig;
            else
                return GetLevelConfigByIndex(CurentLevelConfigIndex);
        }
        public LevelConfig GetLevelConfigByIndex(int index)
        {
            return _levelConfigContainer.LevelConfigs[Mathf.Clamp(index, 0, LevelConfigCount)];
        }
    }
}
