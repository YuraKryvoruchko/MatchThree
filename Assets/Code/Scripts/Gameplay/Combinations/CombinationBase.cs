using UnityEngine;

using SimilarCellsNumber = Core.Gameplay.GameField.SimilarCellsNumber;

namespace Core.Gameplay
{
    public abstract class CombinationBase
    {
        protected GameField GameField { get; private set; }

        public int Score { get; private set; }

        public CombinationBase(GameField gameField, int score)
        {
            GameField = gameField;
            Score = score;
        }

        public abstract bool CanProcessedCombination(in SimilarCellsNumber combinationResult);
        public abstract bool TryProcess(Vector2Int startPosition, in SimilarCellsNumber combinationResult);
    }
}
