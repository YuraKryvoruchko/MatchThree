using Zenject;

namespace Core.Infrastructure.Service.Saving
{
    public class SavingService : ISavingService, IInitializable
    {
        private const string FILE_NAME = "gamedata.bin";

        private const string LONG_MODE_LEVEL_KEY = "longlevel";

        public void Initialize()
        {
            SaveSystem.Initialize(FILE_NAME);
        }

        public void SaveToDisk()
        {
            SaveSystem.SaveToDisk();
        }
        public void SaveLevelProgress(int levelId, float progress)
        {
            SaveSystem.SetFloat(levelId.ToString(), progress);
        }
        public void SaveLongModeLevelProgress(int numberOfScore)
        {
            SaveSystem.SetInt(LONG_MODE_LEVEL_KEY, numberOfScore);
        }
        public float GetLevelProgress(int levelId)
        {
            return SaveSystem.GetFloat(levelId.ToString());
        }
        public int GetLongModeProgress()
        {
            return SaveSystem.GetInt(LONG_MODE_LEVEL_KEY);
        }
    }
}
