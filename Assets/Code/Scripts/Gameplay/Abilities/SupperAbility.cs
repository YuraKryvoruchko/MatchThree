using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Core.VFX.Abilities;
using Core.Infrastructure.Service.Audio;
using Core.VFX;

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

            List<Cell> cellList = swipedCell.IsSpecial && coreCell.IsSpecial ?
                _gameField.GetByСondition((cell) => cell != null && !cell.IsStatic && !cell.IsExplode) :
                _gameField.GetAllOfType(swipedCell.Type);

            Vector3[] cellPositions = new Vector3[cellList.Count];
            for (int i = 0; i < cellList.Count; i++)
                cellPositions[i] = cellList[i].transform.position;

            _audioSourceInstance = _audioService.PlayWithSource(_elementCapturingEvent);

            _abilityEffect = await GetEffect(coreCell.transform.position);
            _abilityEffect.OnComplete += ReleaseEffect;
            _abilityEffect.OnStoped += ReleaseEffect;

            UniTask[] tasks = new UniTask[cellList.Count + 1];
            bool cellsExploded = false;
            _abilityEffect.SetParameters(new SupperAbilityEffect.SupperAbilityVFXParameters(cellPositions, null, async () =>
            {
                _audioService.ReleaseSource(_audioSourceInstance);
                for (int i = 0; i < cellList.Count; i++)
                {
                    if (!cellList[i].IsExplode)
                        tasks[i] = _gameField.ExplodeCellAsync(_gameField.WorldPositionToCell(cellList[i].transform.position));
                }
                tasks[tasks.Length - 1] = _gameField.ExplodeCellAsync(_gameField.WorldPositionToCell(coreCell.transform.position));
                await UniTask.WhenAll(tasks);
                cellsExploded = true;
            }));
            _abilityEffect.Play().Forget();

            await UniTask.WaitUntil(() => cellsExploded);
        }

        private async UniTask<SupperAbilityEffect> GetEffect(Vector3 position)
        {
            return (await Addressables.InstantiateAsync(_supperAbilityEffectReference,
                position, Quaternion.identity)).GetComponent<SupperAbilityEffect>();
        }
        private void ReleaseEffect(IBasicVFXEffect basicEffect)
        {
            basicEffect.OnComplete -= ReleaseEffect;
            basicEffect.OnStoped -= ReleaseEffect;
            SupperAbilityEffect effect = basicEffect as SupperAbilityEffect;
            Addressables.ReleaseInstance(effect.gameObject);
        }
    }
}
