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
        private List<LightingBoltEffect> _lightingBoltEffects;

        private int _lightningBoltCount;
        private IAbility _severalAbility;

        private IAudioService _audioService;
        private ClipEvent _clipEvent;
        private List<SourceInstance> _audioSourceInstancies;

        public LightningBoltAbility(IAudioService audioService, ClipEvent hitClipEvent, AssetReference lightingBoltEffectPrefab,
            int lightningBoltCount, IAbility severalAbility = null)
        {
            _audioService = audioService;
            _clipEvent = hitClipEvent;
            _lightingBoltEffectPrefab = lightingBoltEffectPrefab;
            _lightningBoltCount = lightningBoltCount;
            _severalAbility = severalAbility;

            _audioSourceInstancies = new List<SourceInstance>();
            _lightingBoltEffects = new List<LightingBoltEffect>();
        }

        void IAbility.Init(GameField gameField)
        {
            _gameField = gameField;
        }
        void IAbility.SetPause(bool isPause)
        {
            foreach(LightingBoltEffect effect in _lightingBoltEffects)
                effect.Pause(isPause);
            foreach(SourceInstance source in _audioSourceInstancies)
                source.Pause(isPause);
        }
        async UniTask IAbility.Execute(Vector2Int swipedCellPosition, Vector2Int abilityPosition)
        {
            if (_severalAbility != null)
                _severalAbility.Init(_gameField);

            SourceInstance audioInstance = _audioService.PlayWithSource(_clipEvent, false);
            _audioSourceInstancies.Add(audioInstance);

            Cell swipedCell = _gameField.GetCell(swipedCellPosition);
            Cell abilityCell = _gameField.GetCell(abilityPosition);
            List<Cell> cells = abilityCell.IsSpecial && swipedCell.IsSpecial ? 
                _gameField.GetByСondition((cell) => cell != null && !cell.IsStatic && !cell.IsSpecial && !cell.IsExplode) :
                _gameField.GetAllOfType(swipedCell.Type);

            if(swipedCellPosition != abilityPosition)
                _gameField.ExplodeCell(abilityPosition).Forget();

            LightingBoltEffect lightingBoltEffect = (await Addressables.InstantiateAsync(_lightingBoltEffectPrefab))
                .GetComponent<LightingBoltEffect>();
            _lightingBoltEffects.Add(lightingBoltEffect);

            Cell randomCell = swipedCell;
            for (int i = 0; i < _lightningBoltCount && cells.Count > 0; i++)
            {
                cells.Remove(randomCell);

                Vector3 startPosition = randomCell.transform.position;
                startPosition.y = 5;

                lightingBoltEffect.Play(startPosition, randomCell.transform.position);
                audioInstance.Play();
                if (_severalAbility == null)
                    await _gameField.ExplodeCell(_gameField.WorldPositionToCell(randomCell.transform.position));
                else
                    await _severalAbility.Execute(swipedCellPosition, _gameField.WorldPositionToCell(randomCell.transform.position));

                if(cells.Count > 0)
                    randomCell = cells[Random.Range(0, cells.Count - 1)];
            }

            _lightingBoltEffects.Remove(lightingBoltEffect);
            Addressables.ReleaseInstance(lightingBoltEffect.gameObject);
            _audioSourceInstancies.Remove(audioInstance);
            _audioService.ReleaseSource(audioInstance);
        }
    }
}
