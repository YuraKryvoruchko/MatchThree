using System;
using Zenject;
using Core.Gameplay;
using Core.Infrastructure.Service.Saving;
using Core.Infrastructure.Service;

namespace Core.Infrastructure.Gameplay
{
    public class LongModeSimulation : IGameModeSimulation, IInitializable, IDisposable
    {
        private GameField _gameField;

        private GameScoreTracking _gameScoreObserver;

        private ISavingService _savingService;

        private ILevelService _levelService;

        public event Action OnBlockGame;
        public event Action OnGameComplete;

        public LongModeSimulation(GameField gameField, GameScoreTracking gameScoreObserver, ISavingService savingService, ILevelService levelService)
        {
            _gameField = gameField;
            _gameScoreObserver = gameScoreObserver;
            _savingService = savingService;
            _levelService = levelService;
        }

        void IInitializable.Initialize()
        {
            _gameField.Init(_levelService.GetCurrentLevelConfig().GameFieldConfig);
        }
        void IDisposable.Dispose()
        {
            _gameField.Deinit();
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
