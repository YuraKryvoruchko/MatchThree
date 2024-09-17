using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Core.Gameplay
{
    public interface IAbility
    {
        void Init(GameField gameField);
        void SetPause(bool isPause);
        bool CanExecute();
        UniTask Execute(Vector2Int cellPosition, Vector2Int abilityPosition, Action<IAbility> callback = null, CancellationToken cancellationToken = default);
    }
}
