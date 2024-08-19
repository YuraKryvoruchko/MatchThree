using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Core.VFX.Abilities;
using Core.Infrastructure.Service.Audio;

namespace Core.Gameplay
{
    public class QuicklySupperAbility : IAbility
    {
        private SupperAbilityEffect _abilityEffectInstance;

        private GameField _gameField;

        private SourceInstance _audioSourceInstance;

        private readonly AssetReference _supperAbilityEffectReference;

        private readonly IAbility _ability;
        private readonly int _creatingAbilityObjectNumber;

        private readonly IAudioService _audioService;
        private readonly ClipEvent _elementCapturingEvent;

        public QuicklySupperAbility(IAudioService audioService, ClipEvent elementCapturingEvent, AssetReference supperAbilityEffectReference,
            IAbility ability, int creatingAbilityObjectNumber)
        {
            _audioService = audioService;
            _elementCapturingEvent = elementCapturingEvent;
            _supperAbilityEffectReference = supperAbilityEffectReference;
            _creatingAbilityObjectNumber = creatingAbilityObjectNumber;
            _ability = ability;
        }

        void IAbility.Init(GameField gameField)
        {
            _gameField = gameField;
        }
        void IAbility.SetPause(bool isPause)
        {
            _abilityEffectInstance.Pause(isPause);
            _audioSourceInstance.Pause(isPause);
        }
        async UniTask IAbility.Execute(Vector2Int swipedCellPosition, Vector2Int abilityPosition)
        {
            Cell swipedCell = _gameField.GetCell(swipedCellPosition);
            Cell coreCell = _gameField.GetCell(abilityPosition);

            _abilityEffectInstance = (await Addressables.InstantiateAsync(_supperAbilityEffectReference,
                coreCell.transform.position, Quaternion.identity)).GetComponent<SupperAbilityEffect>();

            _audioSourceInstance = _audioService.PlayWithSource(_elementCapturingEvent);

            Cell[] cellList = _gameField.GetByСondition((cell) => !cell.IsStatic && !cell.IsExplode && !cell.IsSpecial).ToArray();
            Vector2Int[] cellPositions = new Vector2Int[_creatingAbilityObjectNumber];
            Vector3[] worldCellPositions = new Vector3[_creatingAbilityObjectNumber];

            cellPositions[0] = swipedCellPosition;
            worldCellPositions[0] = swipedCell.transform.position;
            for (int i = 1; i < _creatingAbilityObjectNumber; i++)
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
            int index = 0;
            _abilityEffectInstance.SetParameters(new SupperAbilityEffect.SupperAbilityVFXParameters(worldCellPositions, (worldPosition) =>
            {
                tasks[index] = _ability.Execute(cellPositions[index], cellPositions[index]);
                index++;
            }, null));
            await _abilityEffectInstance.Play();
            await UniTask.WhenAll(tasks);

            _audioService.ReleaseSource(_audioSourceInstance);
            Addressables.ReleaseInstance(_abilityEffectInstance.gameObject);
        }
    }
}
