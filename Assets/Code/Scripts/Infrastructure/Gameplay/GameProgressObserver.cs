using System;
using Cysharp.Threading.Tasks;
using Zenject;
using Core.Gameplay;
using Core.Infrastructure.Service;
using Core.UI.Gameplay;
using Core.Infrastructure.Service.Saving;

namespace Core.Infrastructure.Gameplay
{
    public class GameProgressObserver : IGameModeSimulation, IInitializable, IDisposable
    {
        private LevelConfig _levelConfig;
        private LevelTaskCompletionChecker _taskCompletionChecker;
        private PlayerMoveObserver _playerMoveObserver;
        private GameScoreObserver _gameScoreObserver;

        private ILevelService _levelService;
        private IWindowService _windowService;
        private ISavingService _savingService;

        private bool _isLevelCompleted;

        public GameProgressObserver(PlayerMoveObserver playerMoveObserver, LevelTaskCompletionChecker levelTaskCompletionChecker,
            GameScoreObserver gameScoreObserver, ILevelService levelService, IWindowService windowService, ISavingService savingService)
        {
            _gameScoreObserver = gameScoreObserver;

            _playerMoveObserver = playerMoveObserver;
            _playerMoveObserver.OnMove += HandleMoveOnField;

            _taskCompletionChecker = levelTaskCompletionChecker;
            _taskCompletionChecker.OnAllTaskCompleted += HandleTaskCompleting;

            _windowService = windowService;
            _levelService = levelService;
            _savingService = savingService;
        }
        public void Initialize()
        {
            _levelConfig = _levelService.GetCurrentLevelConfig();
        }
        public void Dispose()
        {
            _playerMoveObserver.OnMove -= HandleMoveOnField;
            _taskCompletionChecker.OnAllTaskCompleted -= HandleTaskCompleting;
        }

        public void HandleEndGame()
        {
            if (!_isLevelCompleted)
                return;

            if(_savingService.GetLevelProgress(_levelService.CurentLevelConfigIndex) < _taskCompletionChecker.GetProgress())
                _savingService.SaveLevelProgress(_levelService.CurentLevelConfigIndex, _taskCompletionChecker.GetProgress());
        }

        private void HandleMoveOnField()
        {
            if (_playerMoveObserver.Count != _levelConfig.MoveCount)
                return;

            HandleTaskCompleting();
        }
        private void HandleTaskCompleting()
        {
            _isLevelCompleted = true;

            UniTask.Void(async () =>
            {
                CompletePopup completePopup = await _windowService.OpenPopup<CompletePopup>("CompletePopup");
                completePopup.Activate(_taskCompletionChecker.GetProgress(), _gameScoreObserver.CurrentScore).Forget();
            });

            HandleEndGame();
        }
    }
}
