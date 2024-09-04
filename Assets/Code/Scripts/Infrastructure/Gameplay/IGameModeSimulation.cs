using System;

namespace Core.Infrastructure.Gameplay
{
    public interface IGameModeSimulation
    {
        event Action OnGameComplete;

        void HandleEndGame();
    }
}
