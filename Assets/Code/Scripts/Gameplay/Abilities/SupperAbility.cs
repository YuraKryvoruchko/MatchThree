using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Core.VFX.Abilities;
using Core.Infrastructure.Service.Audio;
using System.Net.NetworkInformation;

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
        async UniTask IAbility.Execute(int xPosition, int yPosition)
        {
            Cell cell = _gameField.GetCell(xPosition, yPosition);
            List<Cell> cellList = _gameField.GetAllOfType(cell.Type);
            _abilityEffect = (await Addressables.InstantiateAsync(_supperAbilityEffectReference,
                cell.transform.position, Quaternion.identity)).GetComponent<SupperAbilityEffect>();

            Vector3[] cellPositions = new Vector3[cellList.Count];
            for (int i = 0; i < cellList.Count; i++)
                cellPositions[i] = cellList[i].transform.position;

            _audioSourceInstance = _audioService.PlayWithSource(_elementCapturingEvent);
            UniTask[] tasks = new UniTask[cellList.Count];
            await _abilityEffect.Play(cellPositions, () => 
            {
                _audioService.ReleaseSource(_audioSourceInstance);
                for (int i = 0; i < cellList.Count; i++)
                {
                    if (!cellList[i].IsExplode)
                        tasks[i] = _gameField.ExplodeCell(cellList[i]);
                }
            });
            await UniTask.WhenAll(tasks);

            Addressables.ReleaseInstance(_abilityEffect.gameObject);
        }
    }
}
