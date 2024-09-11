using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Core.VFX.Abilities;
using Core.Infrastructure.Service.Audio;
using System;

using Random = UnityEngine.Random;

namespace Core.Gameplay
{
    public class ReplaycableSupperAbility : BaseSupperAbility
    {
        private readonly CellType _replaceObject;
        private readonly IAbility _ability;
        private readonly int _creatingAbilityObjectNumber;

        public ReplaycableSupperAbility(IAudioService audioService, ClipEvent elementCapturingEvent, AssetReference supperAbilityEffectReference,
            CellType replaceObject, IAbility ability, int creatingAbilityObjectNumber) : 
            base(audioService, elementCapturingEvent, supperAbilityEffectReference)
        {
            _creatingAbilityObjectNumber = creatingAbilityObjectNumber;
            _ability = ability;
            _replaceObject = replaceObject;
        }

        public override async UniTask Execute(Vector2Int swipedCellPosition, Vector2Int abilityPosition, Action<IAbility> callback)
        {
            Cell swipedCell = GameFieldInstance.GetCell(swipedCellPosition);
            Cell coreCell = GameFieldInstance.GetCell(abilityPosition);

            Cell[] cellList = GameFieldInstance.GetByCondition((cell) => cell != null && !cell.IsStatic && !cell.IsExplode && !cell.IsSpecial).ToArray();
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

            _ability.Init(GameFieldInstance);

            AudioClipSource audioSourceInstance = AudioService.PlayWithSource(AudioClipEvent);
            SupperAbilityEffect abilityEffectInstance = (await Addressables.InstantiateAsync(SupperAbilityEffectReference,
                coreCell.transform.position, Quaternion.identity)).GetComponent<SupperAbilityEffect>();

            OnPause += audioSourceInstance.Pause;
            OnPause += abilityEffectInstance.Pause;

            abilityEffectInstance.SetParameters(new SupperAbilityEffect.SupperAbilityVFXParameters(worldCellPositions,
                (worldPosition) =>
                {
                    GameFieldInstance.ReplaceCell(_replaceObject, GameFieldInstance.WorldPositionToCell(worldPosition));
                },
                () =>
                {
                    for (int i = 0; i < _creatingAbilityObjectNumber; i++)
                    {
                        _ability.Execute(cellPositions[i], cellPositions[i]).Forget();
                    }
                    GameFieldInstance.ExplodeCellAsync(GameFieldInstance.WorldPositionToCell(coreCell.transform.position)).Forget();
                }));
            await abilityEffectInstance.Play();

            OnPause -= audioSourceInstance.Pause;
            OnPause -= abilityEffectInstance.Pause;
            AudioService.ReleaseSource(audioSourceInstance);
            Addressables.ReleaseInstance(abilityEffectInstance.gameObject);

            callback?.Invoke(this);
        }
    }
}
