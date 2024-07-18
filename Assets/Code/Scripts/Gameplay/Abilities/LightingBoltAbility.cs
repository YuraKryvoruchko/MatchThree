using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Core.VFX.Abilities;

namespace Core.Gameplay
{
    public class LightingBoltAbility : IAbility
    {
        private GameField _gameField;
        private AssetReference _lightingBoltEffectPrefab;

        public LightingBoltAbility(AssetReference lightingBoltEffectPrefab)
        {
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
            await _gameField.ExplodeCell(xPosition, yPosition);

            Addressables.ReleaseInstance(lightingBoltEffect.gameObject);
        }
    }
}
