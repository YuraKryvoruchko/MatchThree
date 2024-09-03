using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Core.Gameplay
{
    public class Cell : MonoBehaviour
    {
        [Header("Basic Settings")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private float _moveSpeedPerSecond;
        
        private CellType _type;

        private CellConfig _config;

        private bool _isStatic = false;
        private bool _isSpecial = false;

        private Tweener _moveTweener;
        private Tweener _explosionTweener;

        public CellType Type { get => _type; }
        public int Score { get => _config.Score; }
        public bool IsMove { get; private set; }
        public bool IsExplode { get; private set; }

        public bool IsStatic { get => _isStatic; private set => _isStatic = value; }
        public bool IsSpecial { get => _isSpecial; private set => _isSpecial = value; }

        private void OnDestroy()
        {
            _moveTweener.Kill();
            _explosionTweener.Kill();
        }

        public void Init(CellConfig config)
        {
            _spriteRenderer.sprite = config.Icon;
            _type = config.Type;
            _isSpecial = config.IsSpecial;
            _isStatic = config.IsStatic;
            _config = config;
        }

        public void MoveTo(Vector3 endPosition, bool inLocal = true, Action<Cell> onComplete = null)
        {
            MoveToWithTask(endPosition, inLocal, onComplete).Forget();
        }
        public async UniTask MoveToWithTask(Vector3 endPosition, bool inLocal = true, Action<Cell> onComplete = null)
        {
            if (_moveTweener.IsActive())
            {
                _moveTweener.ChangeEndValue(endPosition, Vector3.Distance(transform.position, endPosition) / _moveSpeedPerSecond, true).SetEase(Ease.OutBack);
                return;
            }

            IsMove = true;

            if (inLocal)
                _moveTweener = transform.DOLocalMove(endPosition, Vector3.Distance(transform.position, endPosition) / _moveSpeedPerSecond).SetEase(Ease.OutBack);
            else
                _moveTweener = transform.DOMove(endPosition, Vector3.Distance(transform.position, endPosition) / _moveSpeedPerSecond).SetEase(Ease.OutBack);

            await _moveTweener.OnComplete(() => 
            {
                IsMove = false;
                onComplete?.Invoke(this);
            }).AsyncWaitForCompletion().AsUniTask();
        }
        public void StopMove()
        {
            IsMove = false;
            _moveTweener.Kill();
        }
        public async UniTask Explode()
        {
            IsExplode = true;
            _explosionTweener = transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
            await _explosionTweener.AsyncWaitForCompletion().AsUniTask();
            IsExplode = false;
        }

        public void SetPause(bool isPause)
        {
            if (isPause)
            {
                _moveTweener.Pause();
                _explosionTweener.Pause();
            }
            else
            {
                _moveTweener.Play();
                _explosionTweener.Play();
            }
        }
    }
}
