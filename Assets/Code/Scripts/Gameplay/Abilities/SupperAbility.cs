using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Core.VFX.Abilities;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Service;

namespace Core.Gameplay
{
    public class SupperAbility : BaseSupperAbility
    {
        private CancellationTokenSource _cancellationTokenSource;

        public SupperAbility(IAudioService audioService, ClipEvent elementCapturingEvent, AssetReferenceGameObject supperAbilityEffectReference) : 
            base(audioService, elementCapturingEvent, supperAbilityEffectReference)
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public override void OnDispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
        public override async UniTask Execute(Vector2Int swipedCellPosition, Vector2Int abilityPosition, Action<IAbility> callback, CancellationToken cancellationToken)
        {
            Cell swipedCell = GameFieldInstance.GetCell(swipedCellPosition);
            Cell coreCell = GameFieldInstance.GetCell(abilityPosition);

            List<Cell> cellList = swipedCell.IsSpecial && coreCell.IsSpecial ?
                GameFieldInstance.GetByCondition((cell) => cell != null && !cell.IsStatic && !cell.IsExplode) :
                GameFieldInstance.GetAllOfType(swipedCell.Type);

            Vector3[] cellPositions = new Vector3[cellList.Count];
            for (int i = 0; i < cellList.Count; i++)
                cellPositions[i] = cellList[i].transform.position;

            AudioClipSource audioSourceInstance = AudioService.PlayWithSource(AudioClipEvent);
            SupperAbilityEffect abilityEffect = null;

            CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, cancellationToken);
            try
            {
                if (SupperAbilityEffectReference.Asset == null)
                    await SupperAbilityEffectReference.GetOrLoad(cancellationTokenSource.Token);
                abilityEffect = GameObject.Instantiate((GameObject)SupperAbilityEffectReference.Asset, 
                    coreCell.transform.position, Quaternion.identity).GetComponent<SupperAbilityEffect>();

                OnPause += audioSourceInstance.Pause;
                OnPause += abilityEffect.Pause;

                abilityEffect.SetParameters(new SupperAbilityEffect.SupperAbilityVFXParameters(cellPositions, null, () =>
                {
                    OnPause -= audioSourceInstance.Pause;
                    audioSourceInstance.Stop();
                    for (int i = 0; i < cellList.Count; i++)
                    {
                        if (!cellList[i].IsExplode)
                            GameFieldInstance.ExplodeCellAsync(GameFieldInstance.WorldPositionToCell(cellList[i].transform.position)).Forget();
                    }
                    GameFieldInstance.ExplodeCellAsync(GameFieldInstance.WorldPositionToCell(coreCell.transform.position)).Forget();
                }));
                await abilityEffect.Play(cancellationTokenSource.Token);

                callback?.Invoke(this);
            }
            finally
            {
                OnPause -= audioSourceInstance.Pause;
                OnPause -= abilityEffect.Pause;
                AudioService.ReleaseSource(audioSourceInstance);
                GameObject.Destroy(abilityEffect.gameObject);

                cancellationTokenSource.Dispose();
            }
        }
    }
}
