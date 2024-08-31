using Core.Gameplay;
using Core.Infrastructure.Service.Saving;

namespace Core.Infrastructure.Gameplay
{
    public class LongModeSimulation : IGameModeSimulation
    {
        private GameScoreObserver _gameScoreObserver;

        private ISavingService _savingService;

        public LongModeSimulation(GameScoreObserver gameScoreObserver,ISavingService savingService)
        {
            _gameScoreObserver = gameScoreObserver;

            _savingService = savingService;
        }

        public void HandleEndGame()
        {
            if (_savingService.GetLongModeProgress() < _gameScoreObserver.CurrentScore)
            {
                _savingService.SaveLongModeLevelProgress(_gameScoreObserver.CurrentScore);
                _savingService.SaveToDisk();
            }
        }
    }
}
