using System;
using UnityEngine;
using DG.Tweening;

namespace Core.VFX.Abilities
{
    public class MagicLineEffect : MonoBehaviour
    {
        [SerializeField] private LineRenderer _line;

        private Vector3 _startPosition;
        private Vector3 _endPosition;

        private Tweener _moveTweener;

        private const int START_POSITION_INDEX = 0;
        private const int END_POSITION_INDEX = 1;
    
        public Vector3 StartPosition { get => _startPosition; }
        public Vector3 EndPosition { get => _endPosition; }


        public void MoveFromAndTo(Vector3 startPosition, Vector3 endPosition, float duration, Action<MagicLineEffect> OnEnd)
        {
            _startPosition = startPosition;
            _endPosition = endPosition;
            _line.SetPosition(START_POSITION_INDEX, _startPosition);
            _moveTweener = DOTween.To(Lerp, 0f, 1f, duration).OnComplete(() => OnEnd?.Invoke(this));
        }
        public void SetPause(bool isPause)
        {
            if (isPause)
                _moveTweener.Pause();
            else
                _moveTweener.Play();
        }

        private void Lerp(float progress)
        {
            _line.SetPosition(END_POSITION_INDEX, Vector3.Lerp(_startPosition, _endPosition, progress));
        }
    }
}
