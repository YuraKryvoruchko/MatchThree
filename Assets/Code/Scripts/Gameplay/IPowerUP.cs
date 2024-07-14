using Cysharp.Threading.Tasks;

namespace Core.Gameplay
{
    public interface IPowerUp
    {
        void Init(GameField gameField);
        UniTask Execute(int xPosition, int yPosition);
    }
}
