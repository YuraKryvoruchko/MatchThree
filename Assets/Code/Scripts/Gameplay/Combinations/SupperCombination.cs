using UnityEngine;

using SimilarCellsNumber = Core.Gameplay.GameField.SimilarCellsNumber;

namespace Core.Gameplay
{
    public class SupperCombination : CombinationBase
    {
        public SupperCombination(GameField gameField, int score) : base(gameField, score)
        { }

        public override bool CanProcessedCombination(in SimilarCellsNumber combinationResult)
        {
            return (combinationResult.UpNumber + combinationResult.DownNumber >= 4)
                || (combinationResult.LeftNumber + combinationResult.RightNumber >= 4);
        }
        public override bool TryProcess(Vector2Int startPosition, in SimilarCellsNumber combinationResult)
        {
            if (combinationResult.UpNumber + combinationResult.DownNumber >= 4)
            {
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.down, combinationResult.UpNumber);
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.up, combinationResult.DownNumber);
                GameField.ExplodeCellAndReplace(startPosition, CellType.Supper).Forget();

                return true;
            }
            else if (combinationResult.LeftNumber + combinationResult.RightNumber >= 4)
            {
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.left, combinationResult.LeftNumber);
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.right, combinationResult.RightNumber);
                GameField.ExplodeCellAndReplace(startPosition, CellType.Supper).Forget();

                return true;
            }

            return false;
        }
    }
}
