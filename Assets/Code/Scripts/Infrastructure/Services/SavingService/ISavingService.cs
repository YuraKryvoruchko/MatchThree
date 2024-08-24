namespace Core.Infrastructure.Service.Saving
{
    public interface ISavingService
    {
        void SaveLevelProgress(int levelId, int progress);
        int GetLevelProgress(int levelId);

        void SaveLongModeLevelProgress(int numberOfScore);
        int GetLongModeProgress();
    }
}
