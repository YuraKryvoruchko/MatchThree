using System;
using UnityEngine;
using Zenject;
using DG.Tweening;
using Core.Gameplay;

namespace Core.UI.Gameplay
{
    public class LevelModeProgressLine : BaseProgressLine
    {
        [Header("Star Settings")]
        [SerializeField] private float _starShowingDelay = 1f;
        [SerializeField] private StarSettings[] _stars;

        private LevelTaskCompletionChecker _levelTaskCompletionChecker;

        [Serializable]
        private class StarSettings
        {
            public GameObject Star;
            public float MinProgress;

            [NonSerialized]
            public Tweener Tweener;
        }

        [Inject]
        private void Construct(LevelTaskCompletionChecker levelTaskCompletionChecker)
        {
            _levelTaskCompletionChecker = levelTaskCompletionChecker;
        }
        protected override void OnStart()
        {
            _levelTaskCompletionChecker.OnExplodeCell += HandleUpdate;
        }
        protected override void OnDestroyObject()
        {
            _levelTaskCompletionChecker.OnExplodeCell -= HandleUpdate;
        }
        protected override void OnHandlePause(bool isPause)
        {
            if (isPause)
            {
                for (int i = 0; i < _stars.Length; i++)
                    _stars[i].Tweener.Pause();
            }
            else
            {
                for (int i = 0; i < _stars.Length; i++)
                    _stars[i].Tweener.Play();
            }
        }
        
        private void HandleUpdate(CellType type, int count)
        {
            float progress = _levelTaskCompletionChecker.GetProgress();
            base.SetSliderValue(progress);

            for (int i = 0; i < _stars.Length; i++)
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
}
