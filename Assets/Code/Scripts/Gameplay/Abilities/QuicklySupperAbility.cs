using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Core.VFX.Abilities;
using Core.Infrastructure.Service.Audio;

using Random = UnityEngine.Random;

namespace Core.Gameplay
{
    public class QuicklySupperAbility : BaseSupperAbility
    {
        private readonly IAbility _ability;
        private readonly int _creatingAbilityObjectNumber;

        public QuicklySupperAbility(IAudioService audioService, ClipEvent elementCapturingEvent, AssetReference supperAbilityEffectReference,
            IAbility ability, int creatingAbilityObjectNumber) : base(audioService, elementCapturingEvent, supperAbilityEffectReference)
        {
            _creatingAbilityObjectNumber = creatingAbilityObjectNumber;
            _ability = ability;
        }

        public override async UniTask Execute(Vector2Int swipedCellPosition, Vector2Int abilityPosition, Action<IAbility> callback)
        {
            Cell swipedCell = GameFieldInstance.GetCell(swipedCellPosition);
            Cell coreCell = GameFieldInstance.GetCell(abilityPosition);

            SupperAbilityEffect abilityEffectInstance = (await Addressables.InstantiateAsync(SupperAbilityEffectReference,
                coreCell.transform.position, Quaternion.identity)).GetComponent<SupperAbilityEffect>();    
            AudioClipSource audioSourceInstance = AudioService.PlayWithSource(AudioClipEvent);

            OnPause += abilityEffectInstance.Pause;
            OnPause += audioSourceInstance.Pause;

            Cell[] cellList = GameFieldInstance.GetByСondition((cell) => cell != null && !cell.IsStatic && !cell.IsExplode && !cell.IsSpecial).ToArray();
            Vector2Int[] cellPositions = new Vector2Int[_creatingAbilityObjectNumber];
            Vector3[] worldCellPositions = new Vector3[_creatingAbilityObjectNumber];

            cellPositions[0] = swipedCellPosition;
            worldCellPositions[0] = swipedCell.transform.position;
            for (int i = 1; i < _creatingAbilityObjectNumber; i++)
            {
                int randomIndex = Random.Range(i, cellList.Length - 1);
                cellPositions[i] = GameFieldInstance.WorldPositionToCell(cellList[randomIndex].transform.position);
                worldCellPositions[i] = cellList[randomIndex].transform.position;

                Cell tmp = cellList[i];
                cellList[i] = cellList[randomIndex];
                cellList[randomIndex] = tmp;
            }

            UniTask[] tasks = new UniTask[cellList.Length + 1];
            _ability.Init(GameFieldInstance);
            int index = 0;
            abilityEffectInstance.SetParameters(new SupperAbilityEffect.SupperAbilityVFXParameters(worldCellPositions, (worldPosition) =>
            {
                _ability.Execute(cellPositions[index], cellPositions[index]).Forget();
                index++;
            }, null));
            await abilityEffectInstance.Play();

            OnPause -= abilityEffectInstance.Pause;
            OnPause -= audioSourceInstance.Pause;
            AudioService.ReleaseSource(audioSourceInstance);
            Addressables.ReleaseInstance(abilityEffectInstance.gameObject);

            callback?.Invoke(this);
        }
    }
}
