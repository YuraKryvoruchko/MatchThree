using System;
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
        [Space]
        [SerializeField] private bool _showStars;
        [SerializeField] private float _starShowingDelay = 0.25f;
        [Header("Components")]
        [SerializeField] private Slider _slider;
        [SerializeField] private StarSettings[] _stars;

        private int _recordValue;

        private Tweener _tweener;

        private GameScoreTracking _gameScoreTracking;
        private IPauseProvider _pauseProvider;

        [Serializable]
        private class StarSettings
        {
            public GameObject Star;
            public float MinProgress;

            [NonSerialized]
            public Tweener Tweener;
        }

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
            float progress = (float)scoreCount / (float)_recordValue;
            if (_tweener.IsActive())
                _tweener.ChangeEndValue(progress, _updateScoreDelay, true);
            else
                _tweener = _slider.DOValue(progress, _updateScoreDelay);

            if (_showStars)
            {
                for(int i = 0; i < _stars.Length; i++)
                {
                    if (_stars[i].MinProgress <= progress && !_stars[i].Star.activeSelf)
                    {
                        _stars[i].Star.SetActive(true);
                        _stars[i].Star.transform.localScale = Vector3.zero;
                        _stars[i].Tweener = _stars[i].Star.transform.DOScale(Vector3.one, _starShowingDelay).SetEase(Ease.OutBack);
                    }
                }
            }
        }
        private void HandlePause(bool isPause)
        {
            if (isPause)
            {
                _tweener.Pause();
                for (int i = 0; i < _stars.Length; i++)
                    _stars[i].Tweener.Pause();
            }
            else
            {
                _tweener.Play();
                for (int i = 0; i < _stars.Length; i++)
                    _stars[i].Tweener.Play();
            }
        }
    }
}
