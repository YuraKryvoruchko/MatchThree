using System;

namespace Core.Infrastructure.Gameplay
{
    public interface IGameModeSimulation
    {
        event Action OnBlockGame;
        event Action OnGameComplete;

        void HandleEndGame();
    }
}
