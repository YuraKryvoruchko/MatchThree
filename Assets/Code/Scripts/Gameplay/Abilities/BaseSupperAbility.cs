using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Core.Infrastructure.Service.Audio;

namespace Core.Gameplay
{
    public abstract class BaseSupperAbility : IAbility, IDisposable
    {
        protected event Action<bool> OnPause;

        protected IAudioService AudioService { get; private set; }
        protected ClipEvent AudioClipEvent { get; private set; }

        protected AssetReferenceGameObject SupperAbilityEffectReference { get; private set; }

        protected GameField GameFieldInstance { get; private set; }

        public BaseSupperAbility(IAudioService audioService, ClipEvent elementCapturingEvent,
            AssetReferenceGameObject supperAbilityEffectReference)
        {
            AudioService = audioService;
            AudioClipEvent = elementCapturingEvent;
            SupperAbilityEffectReference = supperAbilityEffectReference;
        }
        void IDisposable.Dispose()
        {
            GameFieldInstance.OnPause -= SetPause;
            if (SupperAbilityEffectReference.IsValid())
                SupperAbilityEffectReference.ReleaseAsset();
        }

        public void Init(GameField gameField)
        {
            if (GameFieldInstance != null)
                GameFieldInstance.OnPause -= SetPause;

            GameFieldInstance = gameField;
            GameFieldInstance.OnPause += SetPause;
        }
        public void SetPause(bool isPause)
        {
            OnPause?.Invoke(isPause);
        }

        public abstract void OnDispose();
        public abstract UniTask Execute(Vector2Int swipedCellPosition, Vector2Int abilityPosition, Action<IAbility> callback, CancellationToken cancellationToken);
    }
}
