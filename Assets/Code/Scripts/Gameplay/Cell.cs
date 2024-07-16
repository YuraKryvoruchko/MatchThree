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

        private Vector3 _startPosition;
        private Vector3 _endPosition;
        private float _distance;
        private float _maxTime;
        private float _currentTime;
        private float _progress;

        public CellType Type { get => _type; }
        public int Score { get => _config.Score; }
        public bool IsMove { get; private set; }
        public bool IsExplode { get; private set; }

        public bool IsStatic { get => _isStatic; private set => _isStatic = value; }
        public bool IsSpecial { get => _isSpecial; private set => _isSpecial = value; }

        public void Init(CellConfig config)
        {
            _spriteRenderer.sprite = config.Icon;
            _type = config.Type;
            _isSpecial = config.IsSpecial;
            _isStatic = config.IsStatic;
            _config = config;
        }

        public async void MoveTo(Vector3 endPosition, bool inLocal = true, Action<Cell> onComplete = null)
        {
            if (IsMove)
                SetupMoveParameters(endPosition, inLocal);
            else
                await MoveToWithTask(endPosition, inLocal, onComplete);
        }
        public async UniTask MoveToWithTask(Vector3 endPosition, bool inLocal = true, Action<Cell> onComplete = null)
        {
            SetupMoveParameters(endPosition, inLocal);

            IsMove = true;
            while (_progress < 1)
            {
                _currentTime += Time.deltaTime;
                _progress += _currentTime / _maxTime;
                if(inLocal)
                    transform.localPosition = Vector3.Lerp(_startPosition, _endPosition, _progress);
                else
                    transform.position = Vector3.Lerp(_startPosition, _endPosition, _progress);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            if (inLocal)
                transform.localPosition = _endPosition;
            else
                transform.position = _endPosition;
            IsMove = false;
            onComplete?.Invoke(this);
        }

        public async UniTask Explode()
        {
            IsExplode = true;
            await transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).AsyncWaitForCompletion().AsUniTask();
            IsExplode = false;
        }

        private void SetupMoveParameters(Vector3 endPosition, bool inLocal = true)
        {
            _endPosition = endPosition;
            if (inLocal)
                _startPosition = transform.localPosition;
            else
                _startPosition = transform.position;

            _distance = Vector3.Distance(_startPosition, endPosition);
            _maxTime = _distance / _moveSpeedPerSecond;
            _progress = 0f;
            _currentTime = 0f;
        }
    }
}
