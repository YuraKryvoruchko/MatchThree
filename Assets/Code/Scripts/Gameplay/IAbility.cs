using Cysharp.Threading.Tasks;

namespace Core.Gameplay
{
    public interface IAbility
    {
        void Init(GameField gameField);
        UniTask Execute(int xPosition, int yPosition);
    }
}
