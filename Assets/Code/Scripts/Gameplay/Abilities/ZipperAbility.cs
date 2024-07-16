using Cysharp.Threading.Tasks;

namespace Core.Gameplay
{
    public class ZipperAbility : IAbility
    {
        private GameField _gameField;

        void IAbility.Init(GameField gameField)
        {
            _gameField = gameField;
        }
        async UniTask IAbility.Execute(int xPosition, int yPosition)
        {
            await _gameField.ExplodeCell(xPosition, yPosition);
        }
    }
}
