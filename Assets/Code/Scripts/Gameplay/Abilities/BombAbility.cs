using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Core.Infrastructure.Service.Audio;
using Core.VFX;
using Core.Infrastructure.Service;
using System.Threading;

namespace Core.Gameplay
{
    public class BombAbility : IAbility, IDisposable
    {
        private AssetReferenceGameObject _bombEffectReference;

        private int _lineLength;

        private GameField _gameField;

        private IAudioService _audioService;
        private ClipEvent _explosiveEvent;

        private CancellationTokenSource _cancellationTokenSource;

        private event Action<bool> OnPause;

        public BombAbility(IAudioService audioService, BombAbilityConfig config)
        {
            _lineLength = config.LineLength;
            _audioService = audioService;
            _explosiveEvent = config.ExplosiveEvent;
            _bombEffectReference = config.VfxPrefab;
            _cancellationTokenSource = new CancellationTokenSource();
        }
        void IDisposable.Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;

            if (_bombEffectReference.IsValid())
                _bombEffectReference.ReleaseAsset();    
        }

        public void Init(GameField gameField)
        {
            if (_gameField != null)
                _gameField.OnPause -= SetPause;

            _gameField = gameField;
            _gameField.OnPause += SetPause;
        }
        public void SetPause(bool isPause)
        {
            OnPause?.Invoke(isPause);
        }
        public async UniTask Execute(Vector2Int swipedCellPosition, Vector2Int abilityPosition, Action<IAbility> callback, CancellationToken cancellationToken)
        {
            Vector3 cellPosition = _gameField.CellPositionToWorld(abilityPosition);
            AudioClipSource audioSourceInstance = _audioService.PlayWithSource(_explosiveEvent);
            IBasicVFXEffect bombVFXEffect = null;

            CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, cancellationToken);
            try
            {
                if (_bombEffectReference.Asset == null)
                    await _bombEffectReference.GetOrLoad(tokenSource.Token);

                bombVFXEffect = GameObject.Instantiate((GameObject)_bombEffectReference.Asset, 
                    cellPosition, Quaternion.identity).GetComponent<IBasicVFXEffect>();

                OnPause += bombVFXEffect.Pause;
                OnPause += audioSourceInstance.Pause;

                UniTask explosiveVFXAnimationTask = bombVFXEffect.Play(tokenSource.Token);

                int lengthFromBombCell = (_lineLength - 1) / 2, taskArrayIndex = 0;
                for (int i = -lengthFromBombCell; i <= lengthFromBombCell; i++)
                {
                    for (int j = -lengthFromBombCell; j <= lengthFromBombCell; j++, taskArrayIndex++)
                    {
                        _gameField.ExplodeCellAsync(new Vector2Int(abilityPosition.x + i, abilityPosition.y + j)).Forget();
                    }
                }
                await explosiveVFXAnimationTask;

                callback?.Invoke(this);
            }
            finally
            {
                OnPause -= audioSourceInstance.Pause;
                _audioService.ReleaseSource(audioSourceInstance);
                if(bombVFXEffect != null)
                {
                    OnPause -= bombVFXEffect.Pause;
                    GameObject.Destroy(bombVFXEffect as MonoBehaviour);
                }
                tokenSource.Dispose();
            }
        }
    }
}
