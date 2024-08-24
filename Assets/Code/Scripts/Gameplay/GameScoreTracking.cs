using Core.Infrastructure.Service.Saving;
using System;
using Zenject;

namespace Core.Gameplay
{
    public class GameScoreTracking : IInitializable, IDisposable
    {
        private int _currentScoreCount;

        private GameField _gameField;

        private ISavingService _savingService;

        public int CurrentScoreCount { get => _currentScoreCount; }

        public event Action<int> OnUpdateScoreCount;

        public GameScoreTracking(GameField gameField, ISavingService savingService)
        {
            _gameField = gameField;
            _savingService = savingService;
        }

        void IInitializable.Initialize()
        {
            _gameField.OnExplodeCellWithScore += HandleExplodeCell;
        }
        void IDisposable.Dispose()
        {
            _gameField.OnExplodeCellWithScore -= HandleExplodeCell;

            _savingService.SaveLongModeLevelProgress(_currentScoreCount);
            _savingService.SaveToDisk();
        }

        private void HandleExplodeCell(int score)
        {
            _currentScoreCount += score;
            OnUpdateScoreCount?.Invoke(_currentScoreCount);
        }
    }
}
