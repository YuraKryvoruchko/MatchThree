using System;
using Cysharp.Threading.Tasks;
using Zenject;
using Core.Gameplay;
using Core.Infrastructure.Service;
using Core.Infrastructure.Service.Saving;

namespace Core.Infrastructure.Gameplay
{
    public class LevelModeSimulation : IGameModeSimulation, IInitializable, IDisposable
    {
        private GameField _gameField;
        private LevelConfig _levelConfig;
        private LevelTaskCompletionChecker _taskCompletionChecker;
        private PlayerMoveTracking _playerMoveTracking;

        private ILevelService _levelService;
        private ISavingService _savingService;

        private bool _isLevelCompleted;

        public event Action OnBlockGame;
        public event Action OnGameComplete;

        public LevelModeSimulation(GameField gameField, PlayerMoveTracking playerMoveTracking, LevelTaskCompletionChecker levelTaskCompletionChecker,
            ILevelService levelService, ISavingService savingService)
        {
            _gameField = gameField;

            _playerMoveTracking = playerMoveTracking;

            _taskCompletionChecker = levelTaskCompletionChecker;

            _levelService = levelService;
            _savingService = savingService;
        }
        void IInitializable.Initialize()
        {
            _levelConfig = _levelService.GetCurrentLevelConfig();
            _playerMoveTracking.OnMove += HandleMoveOnField;
            _taskCompletionChecker.OnAllTaskCompleted += HandleTaskCompleting;

            _gameField.Init();
        }
        void IDisposable.Dispose()
        {
            _gameField.Deinit();

            _playerMoveTracking.OnMove -= HandleMoveOnField;
            _taskCompletionChecker.OnAllTaskCompleted -= HandleTaskCompleting;
        }

        public void HandleEndGame()
        {
            if (!_isLevelCompleted)
                return;

            if(_savingService.GetLevelProgress(_levelService.CurentLevelConfigIndex) < _taskCompletionChecker.GetProgress())
            {
                _savingService.SaveLevelProgress(_levelService.CurentLevelConfigIndex, _taskCompletionChecker.GetProgress());
                _savingService.SaveToDisk();
            }
        }

        private void HandleMoveOnField()
        {
            if (_playerMoveTracking.Count != _levelConfig.MoveCount)
                return;

            HandleTaskCompleting();
        }
        private void HandleTaskCompleting()
        {
            _isLevelCompleted = true;

            _gameField.SetSwipeHandlingStatus(false);
            OnBlockGame?.Invoke();

            UniTask.Void(async () =>
            {
                await UniTask.WaitWhile(() => _gameField.IsBoardPlay);
                
                HandleEndGame();
                OnGameComplete?.Invoke();
            });
        }
    }
}
