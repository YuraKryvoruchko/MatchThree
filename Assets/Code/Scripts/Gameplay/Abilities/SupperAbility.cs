using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Core.VFX.Abilities;
using Core.Infrastructure.Service.Audio;
using System;

namespace Core.Gameplay
{
    public class SupperAbility : IAbility
    {
        private AssetReference _supperAbilityEffectReference;

        private GameField _gameField;

        private IAudioService _audioService;
        private ClipEvent _elementCapturingEvent;

        private event Action<bool> OnPause;

        public SupperAbility(IAudioService audioService, ClipEvent elementCapturingEvent, AssetReference supperAbilityEffectReference)
        {
            _audioService = audioService;
            _elementCapturingEvent = elementCapturingEvent;
            _supperAbilityEffectReference = supperAbilityEffectReference;
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
        public async UniTask Execute(Vector2Int swipedCellPosition, Vector2Int abilityPosition)
        {
            Cell swipedCell = _gameField.GetCell(swipedCellPosition);
            Cell coreCell = _gameField.GetCell(abilityPosition);

            List<Cell> cellList = swipedCell.IsSpecial && coreCell.IsSpecial ?
                _gameField.GetByСondition((cell) => cell != null && !cell.IsStatic && !cell.IsExplode) :
                _gameField.GetAllOfType(swipedCell.Type);

            Vector3[] cellPositions = new Vector3[cellList.Count];
            for (int i = 0; i < cellList.Count; i++)
                cellPositions[i] = cellList[i].transform.position;

            SourceInstance audioSourceInstance = _audioService.PlayWithSource(_elementCapturingEvent);
            SupperAbilityEffect abilityEffect = (await Addressables.InstantiateAsync(_supperAbilityEffectReference,
                coreCell.transform.position, Quaternion.identity)).GetComponent<SupperAbilityEffect>();

            OnPause += audioSourceInstance.Pause;
            OnPause += abilityEffect.Pause;

            abilityEffect.SetParameters(new SupperAbilityEffect.SupperAbilityVFXParameters(cellPositions, null, () =>
            {
                OnPause -= audioSourceInstance.Pause;
                _audioService.ReleaseSource(audioSourceInstance);
                for (int i = 0; i < cellList.Count; i++)
                {
                    if (!cellList[i].IsExplode)
                        _gameField.ExplodeCellAsync(_gameField.WorldPositionToCell(cellList[i].transform.position)).Forget();
                }
                _gameField.ExplodeCellAsync(_gameField.WorldPositionToCell(coreCell.transform.position)).Forget();
            }));
            await abilityEffect.Play();

            OnPause -= abilityEffect.Pause;
            Addressables.ReleaseInstance(abilityEffect.gameObject);
        }
    }
}
