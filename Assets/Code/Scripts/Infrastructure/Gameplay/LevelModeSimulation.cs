using System;
using Cysharp.Threading.Tasks;
using Zenject;
using Core.Gameplay;
using Core.Infrastructure.Service;
using Core.UI.Gameplay;
using Core.Infrastructure.Service.Saving;

namespace Core.Infrastructure.Gameplay
{
    public class LevelModeSimulation : IGameModeSimulation, IInitializable, IDisposable
    {
        private LevelConfig _levelConfig;
        private LevelTaskCompletionChecker _taskCompletionChecker;
        private PlayerMoveTracking _playerMoveTracking;
        private GameScoreTracking _gameScoreTracking;

        private ILevelService _levelService;
        private IWindowService _windowService;
        private ISavingService _savingService;

        private bool _isLevelCompleted;

        public LevelModeSimulation(PlayerMoveTracking playerMoveTracking, LevelTaskCompletionChecker levelTaskCompletionChecker,
            GameScoreTracking gameScoreTracking, ILevelService levelService, IWindowService windowService, ISavingService savingService)
        {
            _gameScoreTracking = gameScoreTracking;

            _playerMoveTracking = playerMoveTracking;
            _playerMoveTracking.OnMove += HandleMoveOnField;

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

            UniTask.Void(async () =>
            {
                CompletePopup completePopup = await _windowService.OpenPopup<CompletePopup>("CompletePopup");
                completePopup.Activate(_taskCompletionChecker.GetProgress(), _gameScoreTracking.CurrentScore).Forget();
            });

            HandleEndGame();
        }
    }
}
