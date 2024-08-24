using UnityEngine;
using UnityEngine.UI;
using Zenject;
using DG.Tweening;
using Core.Gameplay;
using Core.Infrastructure.Service.Saving;
using Core.Infrastructure.Service.Pause;

namespace Core.UI.Gameplay
{
    public class UILongModeProgressLine : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _updateScoreDelay = 0.25f;
        [Header("Components")]
        [SerializeField] private Slider _slider;

        private int _recordValue;

        private Tweener _tweener;

        private GameScoreTracking _gameScoreTracking;
        private IPauseProvider _pauseProvider;

        [Inject]
        private void Construct(GameScoreTracking gameScoreTracking, ISavingService savingService, IPauseProvider pauseProvider)
        {
            _gameScoreTracking = gameScoreTracking;
            _gameScoreTracking.OnUpdateScoreCount += HandleUpdateScoreCount;
            _pauseProvider = pauseProvider;
            _pauseProvider.OnPause += HandlePause;

            _recordValue = savingService.GetLongModeProgress();
        }
        private void Start()
        {
            _slider.value = 0f;
        }
        private void OnDestroy()
        {
            _pauseProvider.OnPause -= HandlePause;
            _gameScoreTracking.OnUpdateScoreCount -= HandleUpdateScoreCount;
        }
        
        private void HandleUpdateScoreCount(int scoreCount)
        {
            if (_tweener.IsActive())
                _tweener.ChangeEndValue((float)scoreCount / (float)_recordValue, _updateScoreDelay, true);
            else
                _tweener = _slider.DOValue((float)scoreCount / (float)_recordValue, _updateScoreDelay);
        }
        private void HandlePause(bool isPause)
        {
            if (isPause)
                _slider.DOPause();
            else
                _slider.DOPlay();
        }
    }
}
