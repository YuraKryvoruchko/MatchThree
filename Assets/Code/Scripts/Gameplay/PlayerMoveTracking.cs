using System;

namespace Core.Gameplay
{
    public class PlayerMoveTracking : IDisposable
    {
        private GameField _gameField;

        private int _count;

        public int Count { get => _count; }

        public event Action OnMove;

        public PlayerMoveTracking(GameField gameField)
        {
            _gameField = gameField;
            _gameField.OnMove += HandleMove;
        }
        public void Dispose()
        {
            _gameField.OnMove -= HandleMove;
        }

        private void HandleMove()
        {
            _count++;
            OnMove?.Invoke();
        }
    }
}
