using System;
using Zenject;

namespace Core.Gameplay
{
    public class GameScoreTracking : IInitializable, IDisposable
    {
        private int _currentScoreCount;

        private GameField _gameField;

        public int CurrentScoreCount { get => _currentScoreCount; }

        public event Action<int> OnUpdateScoreCount;

        public GameScoreTracking(GameField gameField)
        {
            _gameField = gameField;
        }

        void IInitializable.Initialize()
        {
            _gameField.OnExplodeCellWithScore += HandleExplodeCell;
        }
        void IDisposable.Dispose()
        {
            _gameField.OnExplodeCellWithScore -= HandleExplodeCell;
        }

        private void HandleExplodeCell(int score)
        {
            _currentScoreCount += score;
            OnUpdateScoreCount?.Invoke(_currentScoreCount);
        }
    }
}
