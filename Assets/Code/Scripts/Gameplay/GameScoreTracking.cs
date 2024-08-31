using System;

namespace Core.Gameplay
{
    public class GameScoreTracking : IDisposable
    {
        private GameField _gameField;

        public int CurrentScore { get; private set; }

        public event Action OnUpdate;

        public GameScoreTracking(GameField gameField) 
        {
            _gameField = gameField;
            _gameField.OnExplodeCellWithScore += HandleCellExplosion;
        }
        public void Dispose()
        {
            _gameField.OnExplodeCellWithScore -= HandleCellExplosion;
        }

        private void HandleCellExplosion(int score)
        {
            CurrentScore += score;
            OnUpdate?.Invoke();
        }
    }
}
