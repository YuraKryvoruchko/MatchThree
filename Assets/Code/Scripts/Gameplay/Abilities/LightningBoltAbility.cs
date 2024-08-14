using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Core.VFX.Abilities;
using Core.Infrastructure.Service.Audio;
using System.Collections.Generic;

namespace Core.Gameplay
{
    public class LightningBoltAbility : IAbility
    {
        private GameField _gameField;
        private AssetReference _lightingBoltEffectPrefab;
        private LightingBoltEffect _lightingBoltEffect;

        private int _lightningBoltCount;
        private IAbility _severalAbility;

        private IAudioService _audioService;
        private ClipEvent _clipEvent;
        private SourceInstance _audioSourceInstance;

        public LightningBoltAbility(IAudioService audioService, ClipEvent hitClipEvent, AssetReference lightingBoltEffectPrefab,
            int lightningBoltCount, IAbility severalAbility = null)
        {
            _audioService = audioService;
            _clipEvent = hitClipEvent;
            _lightingBoltEffectPrefab = lightingBoltEffectPrefab;
            _lightningBoltCount = lightningBoltCount;
            _severalAbility = severalAbility;
        }

        void IAbility.Init(GameField gameField)
        {
            _gameField = gameField;
        }
        void IAbility.SetPause(bool isPause)
        {
            _lightingBoltEffect.Pause(isPause);
            _audioSourceInstance.Pause(isPause);
        }
        async UniTask IAbility.Execute(Vector2Int swipedCellPosition, Vector2Int abilityPosition)
        {
            if(_audioSourceInstance == null)
                _audioSourceInstance = _audioService.PlayWithSource(_clipEvent, false);

            _gameField.ExplodeCell(abilityPosition).Forget();

            Cell swipedCell = _gameField.GetCell(swipedCellPosition);
            Cell abilityCell = _gameField.GetCell(abilityPosition);
            List<Cell> cells = swipedCell.Type == abilityCell.Type ? 
                _gameField.GetByСondition((cell) => !cell.IsStatic && !cell.IsSpecial && !cell.IsExplode) :
                _gameField.GetAllOfType(swipedCell.Type);

            _lightingBoltEffect = (await Addressables.InstantiateAsync(_lightingBoltEffectPrefab))
                .GetComponent<LightingBoltEffect>();

            Cell randomCell = swipedCell;
            for (int i = 0; i < _lightningBoltCount && cells.Count > 0; i++)
            {
                cells.Remove(randomCell);

                Vector3 startPosition = randomCell.transform.position;
                startPosition.y = 5;

                _lightingBoltEffect.Play(startPosition, randomCell.transform.position);
                _audioSourceInstance.Play();
                if (_severalAbility == null)
                    await _gameField.ExplodeCell(_gameField.WorldPositionToCell(randomCell.transform.position));
                else
                    await _severalAbility.Execute(swipedCellPosition, abilityPosition);

                if(cells.Count > 0)
                    randomCell = cells[Random.Range(0, cells.Count - 1)];
            }

            Addressables.ReleaseInstance(_lightingBoltEffect.gameObject);       
            _audioService.ReleaseSource(_audioSourceInstance);
            _audioSourceInstance = null;
        }
    }
}
