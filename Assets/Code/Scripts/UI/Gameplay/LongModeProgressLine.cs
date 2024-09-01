using Zenject;
using Core.Gameplay;
using Core.Infrastructure.Service.Saving;

namespace Core.UI.Gameplay
{
    public class LongModeProgressLine : BaseProgressLine
    {
        private int _recordValue;

        private GameScoreTracking _gameScoreTracking;

        [Inject]
        private void Construct(GameScoreTracking gameScoreTracking, ISavingService savingService)
        {
            _gameScoreTracking = gameScoreTracking;
            _gameScoreTracking.OnUpdate += HandleUpdateScoreCount;

            _recordValue = savingService.GetLongModeProgress();
        }

        protected override void OnStart()
        {
            _gameScoreTracking.OnUpdate += HandleUpdateScoreCount;
        }
        protected override void OnDestroyObject()
        {
            _gameScoreTracking.OnUpdate -= HandleUpdateScoreCount;
        }

        protected override void OnHandlePause(bool isPause)
        {       
        }

        private void HandleUpdateScoreCount()
        {
            base.SetSliderValue(_gameScoreTracking.CurrentScore / _recordValue);
        }
    }
}
