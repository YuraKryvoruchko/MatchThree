using Cysharp.Threading.Tasks;

namespace Core.Gameplay
{
    public class BombAbility : IAbility
    {
        private GameField _gameField;

        void IAbility.Init(GameField gameField)
        {
            _gameField = gameField;
        }
        async UniTask IAbility.Execute(int xPosition, int yPosition)
        {
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
        }
    }
}
