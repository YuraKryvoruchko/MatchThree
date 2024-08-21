using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Core.Infrastructure.Service.Audio;
using System;

namespace Core.Gameplay
{
    public abstract class BaseSupperAbility : IAbility
    {
        protected event Action<bool> OnPause;

        protected IAudioService AudioService { get; private set; }
        protected ClipEvent AudioClipEvent { get; private set; }

        protected AssetReference SupperAbilityEffectReference { get; private set; }

        protected GameField GameFieldInstance { get; private set; }

        public BaseSupperAbility(IAudioService audioService, ClipEvent elementCapturingEvent, AssetReference supperAbilityEffectReference)
        {
            AudioService = audioService;
            AudioClipEvent = elementCapturingEvent;
            SupperAbilityEffectReference = supperAbilityEffectReference;
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

        public abstract UniTask Execute(Vector2Int swipedCellPosition, Vector2Int abilityPosition);
    }
}
