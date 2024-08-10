using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Gameplay
{
    public interface IAbility
    {
        void Init(GameField gameField);
        void SetPause(bool isPause);
        UniTask Execute(Vector2Int cellPosition, Vector2Int abilityPosition);
    }
}
