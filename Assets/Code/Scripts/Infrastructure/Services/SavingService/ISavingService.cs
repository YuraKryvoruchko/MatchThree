﻿namespace Core.Infrastructure.Service.Saving
{
    public interface ISavingService
    {
        void SaveToDisk();

        void SaveLevelProgress(int levelId, float progress);
        float GetLevelProgress(int levelId);

        void SaveLongModeLevelProgress(int numberOfScore);
        int GetLongModeProgress();
    }
}
