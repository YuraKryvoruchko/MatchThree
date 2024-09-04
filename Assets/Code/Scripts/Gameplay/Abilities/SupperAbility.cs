using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Core.VFX.Abilities;
using Core.Infrastructure.Service.Audio;

namespace Core.Gameplay
{
    public class SupperAbility : BaseSupperAbility
    {
        public SupperAbility(IAudioService audioService, ClipEvent elementCapturingEvent, AssetReference supperAbilityEffectReference) : 
            base(audioService, elementCapturingEvent, supperAbilityEffectReference)
        {
        }

        public override async UniTask Execute(Vector2Int swipedCellPosition, Vector2Int abilityPosition)
        {
            Cell swipedCell = GameFieldInstance.GetCell(swipedCellPosition);
            Cell coreCell = GameFieldInstance.GetCell(abilityPosition);

            List<Cell> cellList = swipedCell.IsSpecial && coreCell.IsSpecial ?
                GameFieldInstance.GetByСondition((cell) => cell != null && !cell.IsStatic && !cell.IsExplode) :
                GameFieldInstance.GetAllOfType(swipedCell.Type);

            Vector3[] cellPositions = new Vector3[cellList.Count];
            for (int i = 0; i < cellList.Count; i++)
                cellPositions[i] = cellList[i].transform.position;

            AudioClipSource audioSourceInstance = AudioService.PlayWithSource(AudioClipEvent);
            SupperAbilityEffect abilityEffect = (await Addressables.InstantiateAsync(SupperAbilityEffectReference,
                coreCell.transform.position, Quaternion.identity)).GetComponent<SupperAbilityEffect>();

            OnPause += audioSourceInstance.Pause;
            OnPause += abilityEffect.Pause;

            abilityEffect.SetParameters(new SupperAbilityEffect.SupperAbilityVFXParameters(cellPositions, null, () =>
            {
                OnPause -= audioSourceInstance.Pause;
                AudioService.ReleaseSource(audioSourceInstance);
                for (int i = 0; i < cellList.Count; i++)
                {
                    if (!cellList[i].IsExplode)
                        GameFieldInstance.ExplodeCellAsync(GameFieldInstance.WorldPositionToCell(cellList[i].transform.position)).Forget();
                }
                GameFieldInstance.ExplodeCellAsync(GameFieldInstance.WorldPositionToCell(coreCell.transform.position)).Forget();
            }));
            await abilityEffect.Play();

            OnPause -= abilityEffect.Pause;
            Addressables.ReleaseInstance(abilityEffect.gameObject);
        }
    }
}
