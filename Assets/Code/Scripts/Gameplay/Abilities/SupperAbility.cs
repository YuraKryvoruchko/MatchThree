using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Core.VFX.Abilities;
using Core.Infrastructure.Service.Audio;

namespace Core.Gameplay
{
    public class SupperAbility : IAbility
    {
        private AssetReference _supperAbilityEffectReference;
        private SupperAbilityEffect _abilityEffect;

        private GameField _gameField;

        private IAudioService _audioService;
        private ClipEvent _elementCapturingEvent;
        private SourceInstance _audioSourceInstance;

        public SupperAbility(IAudioService audioService, ClipEvent elementCapturingEvent, AssetReference supperAbilityEffectReference)
        {
            _audioService = audioService;
            _elementCapturingEvent = elementCapturingEvent;
            _supperAbilityEffectReference = supperAbilityEffectReference;
        }

        void IAbility.Init(GameField gameField)
        {
            _gameField = gameField;
        }
        void IAbility.SetPause(bool isPause)
        {
            _abilityEffect.Pause(isPause);
            _audioSourceInstance.Pause(isPause);
        }
        async UniTask IAbility.Execute(Vector2Int swipedCellPosition, Vector2Int abilityPosition)
        {
            Cell swipedCell = _gameField.GetCell(swipedCellPosition);
            Cell coreCell = _gameField.GetCell(abilityPosition);

            List<Cell> cellList = swipedCell.Type == coreCell.Type ?
                _gameField.GetByСondition((cell) => !cell.IsStatic && !cell.IsExplode) :
                _gameField.GetAllOfType(swipedCell.Type);

            _abilityEffect = (await Addressables.InstantiateAsync(_supperAbilityEffectReference,
                coreCell.transform.position, Quaternion.identity)).GetComponent<SupperAbilityEffect>();

            Vector3[] cellPositions = new Vector3[cellList.Count];
            for (int i = 0; i < cellList.Count; i++)
                cellPositions[i] = cellList[i].transform.position;

            _audioSourceInstance = _audioService.PlayWithSource(_elementCapturingEvent);
            UniTask[] tasks = new UniTask[cellList.Count + 1];
            await _abilityEffect.Play(cellPositions, null, () => 
            {
                _audioService.ReleaseSource(_audioSourceInstance);
                for (int i = 0; i < cellList.Count; i++)
                {
                    if (!cellList[i].IsExplode)
                        tasks[i] = _gameField.ExplodeCell(_gameField.WorldPositionToCell(cellList[i].transform.position));
                }
                tasks[tasks.Length - 1] = _gameField.ExplodeCell(_gameField.WorldPositionToCell(coreCell.transform.position));
            });
            await UniTask.WhenAll(tasks);

            Addressables.ReleaseInstance(_abilityEffect.gameObject);
        }
    }
    public class ReplaycableSupperAbility : IAbility
    {
        private AssetReference _supperAbilityEffectReference;
        private SupperAbilityEffect _abilityEffect;

        private GameField _gameField;
        private IAbility _ability;

        private IAudioService _audioService;
        private ClipEvent _elementCapturingEvent;
        private SourceInstance _audioSourceInstance;

        public ReplaycableSupperAbility(IAudioService audioService, ClipEvent elementCapturingEvent, AssetReference supperAbilityEffectReference,
            IAbility ability)
        {
            _audioService = audioService;
            _elementCapturingEvent = elementCapturingEvent;
            _supperAbilityEffectReference = supperAbilityEffectReference;
            _ability = ability;
        }

        void IAbility.Init(GameField gameField)
        {
            _gameField = gameField;
        }
        void IAbility.SetPause(bool isPause)
        {
            _abilityEffect.Pause(isPause);
            _audioSourceInstance.Pause(isPause);
        }
        async UniTask IAbility.Execute(Vector2Int swipedCellPosition, Vector2Int abilityPosition)
        {
            Cell swipedCell = _gameField.GetCell(swipedCellPosition);
            Cell coreCell = _gameField.GetCell(abilityPosition);

            _abilityEffect = (await Addressables.InstantiateAsync(_supperAbilityEffectReference,
                coreCell.transform.position, Quaternion.identity)).GetComponent<SupperAbilityEffect>();

            _audioSourceInstance = _audioService.PlayWithSource(_elementCapturingEvent);

            Cell[] cellList = _gameField.GetByСondition((cell) => !cell.IsStatic && !cell.IsExplode && !cell.IsSpecial).ToArray();
            Vector2Int[] cellPositions = new Vector2Int[5];
            Vector3[] worldCellPositions = new Vector3[5];

            cellPositions[0] = swipedCellPosition;
            worldCellPositions[0] = swipedCell.transform.position;
            for (int i = 1; i < 5; i++)
            {
                int randomIndex = Random.Range(i, cellList.Length - 1);
                cellPositions[i] = _gameField.WorldPositionToCell(cellList[randomIndex].transform.position);
                worldCellPositions[i] = cellList[randomIndex].transform.position;

                Cell tmp = cellList[i];
                cellList[i] = cellList[randomIndex];
                cellList[randomIndex] = tmp;
            }

            UniTask[] tasks = new UniTask[cellList.Length + 1];
            _ability.Init(_gameField);
            await _abilityEffect.Play(worldCellPositions, 
                (worldPosition) => 
                {
                    _gameField.ReplaceCell(CellType.Bomb, _gameField.WorldPositionToCell(worldPosition));
                }, 
                () =>
                {
                    for (int i = 0; i < 5; i++)
                    {
                        tasks[i] = _ability.Execute(cellPositions[i], cellPositions[i]);
                    }
                    tasks[tasks.Length - 1] = _gameField.ExplodeCell(_gameField.WorldPositionToCell(coreCell.transform.position));
                }
            );
            await UniTask.WhenAll(tasks);

            _audioService.ReleaseSource(_audioSourceInstance);
            Addressables.ReleaseInstance(_abilityEffect.gameObject);
        }
    }
}
