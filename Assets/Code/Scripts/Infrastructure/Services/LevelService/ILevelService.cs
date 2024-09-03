using Core.Infrastructure.Gameplay;

namespace Core.Infrastructure.Service
{
    public interface ILevelService
    {
        int LevelConfigCount { get; }
        int CurentLevelConfigIndex { get; }

        bool IsLevelConfigSeted();
        bool IsLevelConfigCustom();

        void SetCurrentLevelConfigByIndex(int index);
        void SetCustomLevelConfig(LevelConfig config);
        void ResetLevelConfig();
        LevelConfig GetCurrentLevelConfig();
        LevelConfig GetLevelConfigByIndex(int index);
    }
}
