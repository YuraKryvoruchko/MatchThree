using Cysharp.Threading.Tasks;

namespace Core.Gameplay
{
    public interface IAbility
    {
        void Init(GameField gameField);
        void SetPause(bool isPause);
        UniTask Execute(int xPosition, int yPosition);
    }
}
