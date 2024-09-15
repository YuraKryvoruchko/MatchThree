using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Core.VFX.Abilities;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Service;

namespace Core.Gameplay
{
    public class LightningBoltAbility : IAbility, IDisposable
    {
        private GameField _gameField;
        private AssetReferenceGameObject _lightingBoltEffectPrefab;

        private int _maxLightningBoltCount;
        private IAbility _severalAbility;

        private IAudioService _audioService;
        private ClipEvent _clipEvent;

        private bool _isPaused;

        private CancellationTokenSource _cancellationTokenSource;

        private event Action<bool> OnPause;

        private const float LIGHTNING_DELAY = 0.4F;

        public LightningBoltAbility(IAudioService audioService, LightningBoltAbilityConfig config, IAbility severalAbility = null)
        {
            _audioService = audioService;
            _clipEvent = config.HitEvent;
            _lightingBoltEffectPrefab = config.VFXPrefab;
            _maxLightningBoltCount = config.MaxLightningBoltCount;
            _severalAbility = severalAbility;
            _cancellationTokenSource = new CancellationTokenSource();
        }
        void IDisposable.Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;

            if (_lightingBoltEffectPrefab.IsValid())
                _lightingBoltEffectPrefab.ReleaseAsset();
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
            _isPaused = isPause;
            OnPause?.Invoke(isPause);
        }
        public async UniTask Execute(Vector2Int swipedCellPosition, Vector2Int abilityPosition, Action<IAbility> callback, CancellationToken cancellationToken)
        {
            if (_severalAbility != null)
                _severalAbility.Init(_gameField);

            AudioClipSource audioInstance = _audioService.PlayWithSource(_clipEvent, false);

            Cell swipedCell = _gameField.GetCell(swipedCellPosition);
            Cell abilityCell = _gameField.GetCell(abilityPosition);

            if(swipedCellPosition != abilityPosition)
                _gameField.ExplodeCellAsync(abilityPosition).Forget();

            LightingBoltEffect lightingBoltEffect = null;
            CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, cancellationToken);
            try
            {
                if (_lightingBoltEffectPrefab.Asset == null)
                    await _lightingBoltEffectPrefab.GetOrLoad(tokenSource.Token);
                lightingBoltEffect = GameObject.Instantiate(_lightingBoltEffectPrefab.Asset as GameObject).GetComponent<LightingBoltEffect>();

                OnPause += lightingBoltEffect.Pause;
                OnPause += audioInstance.Pause;

                Func<Cell, bool> condition = abilityCell.IsSpecial && swipedCell.IsSpecial ?
                        (cell) => cell != null && !cell.IsStatic && !cell.IsSpecial && !cell.IsExplode :
                        (cell) => cell != null && !cell.IsStatic && !cell.IsSpecial && !cell.IsExplode && cell.Type == swipedCell.Type;

                Cell randomCell = swipedCell;
                for (int i = 0; i < _maxLightningBoltCount; i++)
                {
                    Vector3 startPosition = randomCell.transform.position;
                    startPosition.y = 5;

                    lightingBoltEffect.Play(startPosition, randomCell.transform.position);
                    audioInstance.Play().Forget();
                    if (_severalAbility == null)
                        _gameField.ExplodeCellAsync(_gameField.WorldPositionToCell(randomCell.transform.position)).Forget();
                    else
                        _severalAbility.Execute(swipedCellPosition, _gameField.WorldPositionToCell(randomCell.transform.position), null, tokenSource.Token).Forget();

                    if (!_gameField.TryGetRandomCellByCondition(condition, out randomCell))
                        break;

                    await UniTask.WaitForSeconds(LIGHTNING_DELAY, cancellationToken: tokenSource.Token);
                    if (_isPaused)
                        await UniTask.WaitWhile(() => _isPaused, cancellationToken: tokenSource.Token);
                }

                callback?.Invoke(this);
            }
            finally
            {
                OnPause -= lightingBoltEffect.Pause;
                OnPause -= audioInstance.Pause;
                GameObject.Destroy(lightingBoltEffect.gameObject);
                _audioService.ReleaseSource(audioInstance);
                tokenSource.Dispose();
            }
        }
    }
}
