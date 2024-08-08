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

        private IAudioService _audioService;
        private ClipEvent _clipEvent;

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
        async UniTask IAbility.Execute(int xPosition, int yPosition)
        {
            LightingBoltEffect lightingBoltEffect = (await Addressables.InstantiateAsync(_lightingBoltEffectPrefab))
                .GetComponent<LightingBoltEffect>();

            Cell cell = _gameField.GetCell(xPosition, yPosition);
            Vector3 startPosition = cell.transform.position;
            startPosition.y = 5;

            lightingBoltEffect.Play(startPosition, cell.transform.position);
            _audioService.PlayOneShot(_clipEvent);
            await _gameField.ExplodeCell(xPosition, yPosition);

            Addressables.ReleaseInstance(lightingBoltEffect.gameObject);
        }
    }
}
