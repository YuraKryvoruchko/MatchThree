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

        private event Action<bool> OnPause;

        public BombAbility(int lineLength, IAudioService audioService, ClipEvent explosiveEvent, AssetReferenceGameObject bombEffectReference)
        {
            _lineLength = lineLength;
            _audioService = audioService;
            _explosiveEvent = explosiveEvent;
            _bombEffectReference = bombEffectReference;
        }
        void IDisposable.Dispose()
        {
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
        public async UniTask Execute(Vector2Int swipedCellPosition, Vector2Int abilityPosition, Action<IAbility> callback)
        {
            Vector3 cellPosition = _gameField.CellPositionToWorld(abilityPosition);
            AudioClipSource audioSourceInstance = _audioService.PlayWithSource(_explosiveEvent);
            IBasicVFXEffect bombVFXEffect = null;
            try
            {
                if (_bombEffectReference.Asset == null)
                    await _bombEffectReference.GetOrLoad();

                bombVFXEffect = GameObject.Instantiate((GameObject)_bombEffectReference.Asset, 
                    cellPosition, Quaternion.identity).GetComponent<IBasicVFXEffect>();

                OnPause += bombVFXEffect.Pause;
                OnPause += audioSourceInstance.Pause;

                UniTask explosiveVFXAnimationTask = bombVFXEffect.Play();

                int lengthFromBombCell = (_lineLength - 1) / 2, taskArrayIndex = 0;
                UniTask[] explodeTasks = new UniTask[_lineLength * _lineLength + 1];
                for (int i = -lengthFromBombCell; i <= lengthFromBombCell; i++)
                {
                    for (int j = -lengthFromBombCell; j <= lengthFromBombCell; j++, taskArrayIndex++)
                    {
                        explodeTasks[taskArrayIndex] = _gameField.ExplodeCellAsync(new Vector2Int(abilityPosition.x + i, abilityPosition.y + j));
                    }
                }
                explodeTasks[^1] = explosiveVFXAnimationTask;
                await UniTask.WhenAll(explodeTasks);
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

                callback?.Invoke(this);
            }
        }
    }
}
