using System;
using UnityEngine;
using Core.Gameplay;

namespace Core.Infrastructure.Gameplay
{
    public class GameProgressObserver : IDisposable
    {
        private GameField _gameField;

        private LevelConfig _levelConfig;
        private LevelTaskCompletionChecker _taskCompletionChecker;

        private int _moveCount;

        public GameProgressObserver(LevelConfig levelConfig, GameField gameField, LevelTaskCompletionChecker levelTaskCompletionChecker)
        {
            _levelConfig = levelConfig;

            _gameField = gameField;
            _gameField.OnMove += HandleMoveOnField;

            _taskCompletionChecker = levelTaskCompletionChecker;
            _taskCompletionChecker.OnAllTaskCompleted += HandleTaskCompleting;
        }
        public void Dispose()
        {
            _gameField.OnMove -= HandleMoveOnField;
            _taskCompletionChecker.OnAllTaskCompleted -= HandleTaskCompleting;
        }

        private void HandleMoveOnField()
        {
            _moveCount++;
            if (_moveCount != _levelConfig.MoveCount)
                return;
                
            Debug.Log("Game Over!");
        }
        private void HandleTaskCompleting()
        {
            Debug.Log("Complete!");
        }
    }
}
