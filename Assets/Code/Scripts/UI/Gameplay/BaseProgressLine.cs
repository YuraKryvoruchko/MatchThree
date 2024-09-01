using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Core.Infrastructure.Service.Pause;

namespace Core.UI.Gameplay
{
    public abstract class BaseProgressLine : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _updateScoreDelay = 0.25f;
        [Space]
        [SerializeField] private bool _showStars;
        [SerializeField] private float _starShowingDelay = 0.25f;
        [Header("Components")]
        [SerializeField] private Slider _slider;

        private Tweener _tweener;

        private IPauseProvider _pauseProvider;

        private void Construct(IPauseProvider pauseProvider)
        {
            _pauseProvider = pauseProvider;
        }
        protected void Start()
        {
            _pauseProvider.OnPause += HandlePause;
            OnStart();
        }
        protected void OnDestroy()
        {
            _pauseProvider.OnPause -= HandlePause;
            OnDestroyObject();
        }

        protected void SetSliderValue(float value)
        {
            if (_tweener.IsActive())
                _tweener.ChangeEndValue(value, _updateScoreDelay, true);
            else
                _tweener = _slider.DOValue(value, _updateScoreDelay);
        }
        private void HandlePause(bool isPause)
        {
            if (isPause)
            {
                _tweener.Pause();
            }
            else
            {
                _tweener.Play();
            }

            OnHandlePause(isPause);
        }

        protected abstract void OnStart();
        protected abstract void OnDestroyObject();
        protected abstract void OnHandlePause(bool isPause);
    }
}
