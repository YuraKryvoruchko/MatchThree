using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Zenject;
using Core.Infrastructure.Service.Pause;
using Core.Gameplay;

namespace Core.UI.Gameplay
{
    public abstract class BaseProgressLine : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _updateSliderValueTime = 0.25f;
        [Header("Components")]
        [SerializeField] private Slider _slider;

        private Tweener _tweener;

        private IPauseProvider _pauseProvider;

        [Inject]
        private void Construct(IPauseProvider pauseProvider)
        {
            _pauseProvider = pauseProvider;
        }
        protected void Start()
        {
            _slider.value = 0f;
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
                _tweener.ChangeEndValue(value, _updateSliderValueTime, true);
            else
                _tweener = _slider.DOValue(value, _updateSliderValueTime);
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
