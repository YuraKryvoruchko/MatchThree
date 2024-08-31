using System;
using UnityEngine;
using Core.Gameplay;
using Core.Infrastructure.Service;
using Cysharp.Threading.Tasks;
using Core.UI.Gameplay;

namespace Core.Infrastructure.Gameplay
{
    public class GameProgressObserver : IDisposable
    {
        private LevelConfig _levelConfig;
        private LevelTaskCompletionChecker _taskCompletionChecker;
        private PlayerMoveObserver _playerMoveObserver;

        private IWindowService _windowService;

        public event Action OnLose;
        public event Action<int> OnComplete;

        public GameProgressObserver(LevelConfig levelConfig, PlayerMoveObserver playerMoveObserver, LevelTaskCompletionChecker levelTaskCompletionChecker,
            IWindowService windowService)
        {
            _levelConfig = levelConfig;

            _playerMoveObserver = playerMoveObserver;
            _playerMoveObserver.OnMove += HandleMoveOnField;

            _taskCompletionChecker = levelTaskCompletionChecker;
            _taskCompletionChecker.OnAllTaskCompleted += HandleTaskCompleting;

            _windowService = windowService;
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

            UniTask.Void(async () =>
            {
                CompletePopup completePopup = await _windowService.OpenPopup<CompletePopup>("CompletePopup");
                completePopup.Activate(_taskCompletionChecker.GetProgress(), 3839).Forget();
            });

            Debug.Log("Game Over!");
        }
        private void HandleTaskCompleting()
        {
            UniTask.Void(async () =>
            {
                CompletePopup completePopup = await _windowService.OpenPopup<CompletePopup>("CompletePopup");
                completePopup.Activate(1f, 3839).Forget();
            });

            Debug.Log("Complete!");
        }
    }
}
