﻿using System;
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
    public class ReplaycableSupperAbility : BaseSupperAbility
    {
        private CancellationTokenSource _cancellationTokenSource;

        private readonly CellType _replaceObject;
        private readonly IAbility _ability;
        private readonly int _creatingAbilityObjectNumber;

        public ReplaycableSupperAbility(IAudioService audioService, ClipEvent elementCapturingEvent, AssetReferenceGameObject supperAbilityEffectReference,
            CellType replaceObject, IAbility ability, int creatingAbilityObjectNumber) : 
            base(audioService, elementCapturingEvent, supperAbilityEffectReference)
        {
            _creatingAbilityObjectNumber = creatingAbilityObjectNumber;
            _ability = ability;
            _replaceObject = replaceObject;
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
            SupperAbilityEffect abilityEffectInstance = null;

            CancellationTokenSource tokenSource = CancellationTokenSource
                .CreateLinkedTokenSource(_cancellationTokenSource.Token, cancellationToken);
            try 
            {
                if (SupperAbilityEffectReference.Asset == null)
                    await SupperAbilityEffectReference.GetOrLoad(tokenSource.Token);

                abilityEffectInstance = GameObject.Instantiate(SupperAbilityEffectReference.Asset as GameObject,
                coreCell.transform.position, Quaternion.identity).GetComponent<SupperAbilityEffect>();

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
                            _ability.Execute(cellPositions[i], cellPositions[i], null, tokenSource.Token).Forget();
                        }
                        GameFieldInstance.ExplodeCellAsync(GameFieldInstance.WorldPositionToCell(coreCell.transform.position)).Forget();
                    }));
                await abilityEffectInstance.Play(tokenSource.Token);

                callback?.Invoke(this);
            }
            finally
            {
                OnPause -= audioSourceInstance.Pause;
                OnPause -= abilityEffectInstance.Pause;
                AudioService.ReleaseSource(audioSourceInstance);
                GameObject.Destroy(abilityEffectInstance.gameObject);

                tokenSource.Dispose();
            }
        }
    }
}
