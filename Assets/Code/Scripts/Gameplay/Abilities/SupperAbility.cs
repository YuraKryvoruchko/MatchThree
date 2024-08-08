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

        private GameField _gameField;

        private IAudioService _audioService;
        private ClipEvent _elementCapturingEvent;

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
        async UniTask IAbility.Execute(int xPosition, int yPosition)
        {
            Cell cell = _gameField.GetCell(xPosition, yPosition);
            List<Cell> cellList = _gameField.GetAllOfType(cell.Type);
            SupperAbilityEffect effect = (await Addressables.InstantiateAsync(_supperAbilityEffectReference,
                cell.transform.position, Quaternion.identity)).GetComponent<SupperAbilityEffect>();

            Vector3[] cellPositions = new Vector3[cellList.Count];
            for (int i = 0; i < cellList.Count; i++)
                cellPositions[i] = cellList[i].transform.position;

            SourceInstance sourceInstance = _audioService.PlayWithSource(_elementCapturingEvent);
            UniTask[] tasks = new UniTask[cellList.Count];
            await effect.Play(cellPositions, () => 
            {
                sourceInstance.Stop();
                _audioService.ReleaseSource(sourceInstance);
                for (int i = 0; i < cellList.Count; i++)
                {
                    if (!cellList[i].IsExplode)
                        tasks[i] = _gameField.ExplodeCell(cellList[i]);
                }
            });
            await UniTask.WhenAll(tasks);

            Addressables.ReleaseInstance(effect.gameObject);
        }
    }
}
