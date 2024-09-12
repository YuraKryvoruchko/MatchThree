using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Core.Infrastructure.Service;

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
        public Vector3 MoveDirection { get; private set; }

        private const Ease MOVE_EASE = Ease.Linear;

        public bool IsStatic { get => _isStatic; private set => _isStatic = value; }
        public bool IsSpecial { get => _isSpecial; private set => _isSpecial = value; }
        public bool IsSpawn { get => _config.IsSpawn; }

        private void OnDestroy()
        {
            _moveTweener.Kill();
            _explosionTweener.Kill();
            _config.Icon.ReleaseAsset();
        }

        public void Init(CellConfig config)
        {
            _type = config.Type;
            _isSpecial = config.IsSpecial;
            _isStatic = config.IsStatic;
            _config = config;

            if (string.IsNullOrEmpty(config.Icon.AssetGUID))
                return;
            UniTask.Void(async () => 
            {
                _spriteRenderer.sprite = await _config.Icon.GetOrLoad();
            });
        }

        public void MoveTo(Vector3 endPosition, bool inLocal = true, Action<Cell> onComplete = null)
        {
            MoveToWithTask(endPosition, inLocal, onComplete).Forget();
        }
        public async UniTask MoveToWithTask(Vector3 endPosition, bool inLocal = true, Action<Cell> onComplete = null)
        {
            SetMoveDirection(endPosition, inLocal);
            if (_moveTweener.IsActive())
            {
                _moveTweener.ChangeEndValue(endPosition, Vector3.Distance(transform.position, endPosition) / _moveSpeedPerSecond, true).SetEase(MOVE_EASE);
                return;
            }

            IsMove = true;

            if (inLocal)
                _moveTweener = transform.DOLocalMove(endPosition, Vector3.Distance(transform.position, endPosition) / _moveSpeedPerSecond).SetEase(MOVE_EASE);
            else
                _moveTweener = transform.DOMove(endPosition, Vector3.Distance(transform.position, endPosition) / _moveSpeedPerSecond).SetEase(MOVE_EASE);

            _moveTweener.OnKill(() =>
            {
                IsMove = false;
                MoveDirection = Vector3.zero;
            });
            _moveTweener.OnComplete(() =>
            {
                _moveTweener.Kill();
                onComplete?.Invoke(this);
            });
            await _moveTweener.AsyncWaitForCompletion().AsUniTask();
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

        private void SetMoveDirection(Vector3 endPosition, bool inLocal)
        {
            if (inLocal)
                MoveDirection = transform.TransformPoint(endPosition) - transform.position;
            else
                MoveDirection = endPosition - transform.position;

            MoveDirection = MoveDirection.normalized;
        }
    }
}
