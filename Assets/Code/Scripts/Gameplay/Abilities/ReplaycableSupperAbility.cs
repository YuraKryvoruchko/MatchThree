using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Core.VFX.Abilities;
using Core.Infrastructure.Service.Audio;
using Core.VFX;

namespace Core.Gameplay
{
    public class ReplaycableSupperAbility : IAbility
    {
        private SupperAbilityEffect _abilityEffectInstance;

        private GameField _gameField;   

        private SourceInstance _audioSourceInstance;

        private readonly AssetReference _supperAbilityEffectReference;

        private readonly CellType _replaceObject;
        private readonly IAbility _ability;
        private readonly int _creatingAbilityObjectNumber;

        private readonly IAudioService _audioService;
        private readonly ClipEvent _elementCapturingEvent;

        public ReplaycableSupperAbility(IAudioService audioService, ClipEvent elementCapturingEvent, AssetReference supperAbilityEffectReference,
            CellType replaceObject, IAbility ability, int creatingAbilityObjectNumber)
        {
            _audioService = audioService;
            _elementCapturingEvent = elementCapturingEvent;
            _supperAbilityEffectReference = supperAbilityEffectReference;
            _creatingAbilityObjectNumber = creatingAbilityObjectNumber;
            _ability = ability;
            _replaceObject = replaceObject;
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

            Cell[] cellList = _gameField.GetByСondition((cell) => cell != null && !cell.IsStatic && !cell.IsExplode && !cell.IsSpecial).ToArray();
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

            bool cellsExploded = false;
            _audioSourceInstance = _audioService.PlayWithSource(_elementCapturingEvent);
            _abilityEffectInstance = (await Addressables.InstantiateAsync(_supperAbilityEffectReference,
                coreCell.transform.position, Quaternion.identity)).GetComponent<SupperAbilityEffect>();
            _abilityEffectInstance.OnComplete += ReleaseEffect;
            _abilityEffectInstance.OnStoped += ReleaseEffect;
            _abilityEffectInstance.SetParameters(new SupperAbilityEffect.SupperAbilityVFXParameters(worldCellPositions,
                (worldPosition) =>
                {
                    _gameField.ReplaceCell(_replaceObject, _gameField.WorldPositionToCell(worldPosition));
                },
                async () =>
                {
                    for (int i = 0; i < _creatingAbilityObjectNumber; i++)
                    {
                        tasks[i] = _ability.Execute(cellPositions[i], cellPositions[i]);
                    }
                    tasks[tasks.Length - 1] = _gameField.ExplodeCell(_gameField.WorldPositionToCell(coreCell.transform.position));
                    await UniTask.WhenAll(tasks);
                    cellsExploded = true;
                }));
            _abilityEffectInstance.Play().Forget();
            await UniTask.WaitUntil(() => cellsExploded);
        }

        private void ReleaseEffect(IBasicVFXEffect basicEffect)
        {
            basicEffect.OnComplete -= ReleaseEffect;
            basicEffect.OnStoped -= ReleaseEffect;
            SupperAbilityEffect effect = basicEffect as SupperAbilityEffect;
            Addressables.ReleaseInstance(effect.gameObject);
            _audioService.ReleaseSource(_audioSourceInstance);
        }
    }
}
