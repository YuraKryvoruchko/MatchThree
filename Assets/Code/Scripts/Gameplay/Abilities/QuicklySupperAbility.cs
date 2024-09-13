using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Core.VFX.Abilities;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Service;

using Random = UnityEngine.Random;

namespace Core.Gameplay
{
    public class QuicklySupperAbility : BaseSupperAbility
    {
        private CancellationTokenSource _cancellationTokenSource;

        private readonly IAbility _ability;
        private readonly int _creatingAbilityObjectNumber;

        public QuicklySupperAbility(IAudioService audioService, ClipEvent elementCapturingEvent, AssetReferenceGameObject supperAbilityEffectReference,
            IAbility ability, int creatingAbilityObjectNumber) : base(audioService, elementCapturingEvent, supperAbilityEffectReference)
        {
            _creatingAbilityObjectNumber = creatingAbilityObjectNumber;
            _ability = ability;
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

            AudioClipSource audioSourceInstance = AudioService.PlayWithSource(AudioClipEvent, false);
            SupperAbilityEffect abilityEffectInstance = null;

            CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, cancellationToken);
            try
            {
                if (SupperAbilityEffectReference.Asset == null)
                    await SupperAbilityEffectReference.GetOrLoad(tokenSource.Token);
                abilityEffectInstance = GameObject.Instantiate((GameObject)SupperAbilityEffectReference.Asset, coreCell.transform.position, Quaternion.identity)
                    .GetComponent<SupperAbilityEffect>();
                    
                OnPause += abilityEffectInstance.Pause;
                OnPause += audioSourceInstance.Pause;

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

                UniTask[] tasks = new UniTask[cellList.Length + 1];
                _ability.Init(GameFieldInstance);
                int index = 0;
                abilityEffectInstance.SetParameters(new SupperAbilityEffect.SupperAbilityVFXParameters(worldCellPositions, (worldPosition) =>
                {
                    _ability.Execute(cellPositions[index], cellPositions[index], null, tokenSource.Token).Forget();
                    index++;
                }, null));

                audioSourceInstance.Play().Forget();
                await abilityEffectInstance.Play(tokenSource.Token);

                callback?.Invoke(this);
            }
            finally
            {
                OnPause -= abilityEffectInstance.Pause;
                OnPause -= audioSourceInstance.Pause;
                AudioService.ReleaseSource(audioSourceInstance);
                GameObject.Destroy(abilityEffectInstance.gameObject);

                tokenSource.Dispose();
            }
        }
    }
}
