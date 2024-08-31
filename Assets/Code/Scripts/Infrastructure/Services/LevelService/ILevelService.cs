using Core.Infrastructure.Gameplay;

namespace Core.Infrastructure.Service
{
    public interface ILevelService
    {
        int LevelConfigCount { get; }
        int CurrentLevelConfigIndex { get; }

        bool IsLevelConfigSeted();

        void SetCurrentLevelConfigByIndex(int index);
        LevelConfig GetCurrentLevelConfig();
        LevelConfig GetLevelConfigByIndex(int index);
    }
}
