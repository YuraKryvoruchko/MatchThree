using Cysharp.Threading.Tasks;

namespace Core.Gameplay
{
    public interface IPowerUP
    {
        void Init(GameField gameField);
        UniTask Execute(int xPosition, int yPosition);
    }
}
