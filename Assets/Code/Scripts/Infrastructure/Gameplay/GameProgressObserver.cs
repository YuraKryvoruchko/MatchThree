using System;
using UnityEngine;
using Core.Gameplay;

namespace Core.Infrastructure.Gameplay
{
    public class GameProgressObserver : IDisposable
    {
        private LevelConfig _levelConfig;
        private LevelTaskCompletionChecker _taskCompletionChecker;
        private PlayerMoveObserver _playerMoveObserver;

        public GameProgressObserver(LevelConfig levelConfig, PlayerMoveObserver playerMoveObserver, LevelTaskCompletionChecker levelTaskCompletionChecker)
        {
            _levelConfig = levelConfig;

            _playerMoveObserver = playerMoveObserver;
            _playerMoveObserver.OnMove += HandleMoveOnField;

            _taskCompletionChecker = levelTaskCompletionChecker;
            _taskCompletionChecker.OnAllTaskCompleted += HandleTaskCompleting;
        }
        public void Dispose()
        {
            _playerMoveObserver.OnMove -= HandleMoveOnField;
            _taskCompletionChecker.OnAllTaskCompleted -= HandleTaskCompleting;
        }

        private void HandleMoveOnField()
        {
            if (_playerMoveObserver.Count != _levelConfig.MoveCount)
                return;
                
            Debug.Log("Game Over!");
        }
        private void HandleTaskCompleting()
        {
            Debug.Log("Complete!");
        }
    }
}
