using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Core.VFX.Abilities;
using Core.Infrastructure.Service.Audio;

namespace Core.Gameplay
{
    public class LightingBoltAbility : IAbility
    {
        private GameField _gameField;
        private AssetReference _lightingBoltEffectPrefab;
        private LightingBoltEffect _lightingBoltEffect;

        private IAudioService _audioService;
        private ClipEvent _clipEvent;
        private SourceInstance _audioSourceInstance;

        public LightingBoltAbility(IAudioService audioService, ClipEvent hitClipEvent, AssetReference lightingBoltEffectPrefab)
        {
            _audioService = audioService;
            _clipEvent = hitClipEvent;
            _lightingBoltEffectPrefab = lightingBoltEffectPrefab;
        }

        void IAbility.Init(GameField gameField)
        {
            _gameField = gameField;
        }
        void IAbility.SetPause(bool isPause)
        {
            _lightingBoltEffect.Pause(isPause);
            _audioSourceInstance.Pause(isPause);
        }
        async UniTask IAbility.Execute(Vector2Int swipedCellPosition, Vector2Int abilityPosition)
        {
            _lightingBoltEffect = (await Addressables.InstantiateAsync(_lightingBoltEffectPrefab))
                .GetComponent<LightingBoltEffect>();

            Cell cell = _gameField.GetCell(abilityPosition.x, abilityPosition.y);
            Vector3 startPosition = cell.transform.position;
            startPosition.y = 5;

            _lightingBoltEffect.Play(startPosition, cell.transform.position);
            _audioSourceInstance = _audioService.PlayWithSource(_clipEvent);
            await _gameField.ExplodeCell(abilityPosition.x, abilityPosition.y);

            _audioService.ReleaseSource(_audioSourceInstance);
            Addressables.ReleaseInstance(_lightingBoltEffect.gameObject);
        }
    }
}
