using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Gameplay
{
    public class BombAbility : IAbility
    {
        private AssetReference _bombEffectReference;

        private GameField _gameField;

        public BombAbility(AssetReference bombEffectReference)
        {
            _bombEffectReference = bombEffectReference;
        }

        void IAbility.Init(GameField gameField)
        {
            _gameField = gameField;
        }
        async UniTask IAbility.Execute(int xPosition, int yPosition)
        {
            Cell bombCell = _gameField.GetCell(xPosition, yPosition);
            ParticleSystem bombEffectInstance = (await Addressables.InstantiateAsync(_bombEffectReference, 
                bombCell.transform.position, Quaternion.identity)).GetComponent<ParticleSystem>();
            bombEffectInstance.Play();
            await UniTask.WhenAll(
                _gameField.ExplodeCell(xPosition, yPosition),
                _gameField.ExplodeCell(xPosition + 1, yPosition),
                _gameField.ExplodeCell(xPosition - 1, yPosition),
                _gameField.ExplodeCell(xPosition, yPosition + 1),
                _gameField.ExplodeCell(xPosition, yPosition - 1),
                _gameField.ExplodeCell(xPosition + 1, yPosition + 1),
                _gameField.ExplodeCell(xPosition + 1, yPosition - 1),
                _gameField.ExplodeCell(xPosition - 1, yPosition + 1),
                _gameField.ExplodeCell(xPosition - 1, yPosition - 1)
            );
            Addressables.ReleaseInstance(bombEffectInstance.gameObject);
        }
    }
}
