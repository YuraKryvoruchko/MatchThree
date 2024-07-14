using Cysharp.Threading.Tasks;

namespace Core.Gameplay
{
    public class BombPowerUp : IPowerUp
    {
        private GameField _gameField;

        void IPowerUp.Init(GameField gameField)
        {
            _gameField = gameField;
        }
        async UniTask IPowerUp.Execute(int xPosition, int yPosition)
        {
            _gameField.ExplodeCell(xPosition, yPosition);
            _gameField.ExplodeCell(xPosition + 1, yPosition);
            _gameField.ExplodeCell(xPosition - 1, yPosition);
            _gameField.ExplodeCell(xPosition, yPosition + 1);
            _gameField.ExplodeCell(xPosition, yPosition - 1);
            _gameField.ExplodeCell(xPosition + 1, yPosition + 1);
            _gameField.ExplodeCell(xPosition + 1, yPosition - 1);
            _gameField.ExplodeCell(xPosition - 1, yPosition + 1);
            _gameField.ExplodeCell(xPosition - 1, yPosition - 1);
        }
    }
}
