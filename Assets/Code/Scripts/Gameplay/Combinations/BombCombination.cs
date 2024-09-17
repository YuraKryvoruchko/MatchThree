using UnityEngine;

using SimilarCellsNumber = Core.Gameplay.GameField.SimilarCellsNumber;

namespace Core.Gameplay
{
    public class BombCombination : CombinationBase
    {
        public BombCombination(GameField gameField, int score) : base(gameField, score)
        { }

        public override bool CanProcessedCombination(in SimilarCellsNumber combinationResult)
        {
            return (combinationResult.UpNumber >= 2 && combinationResult.LeftNumber >= 2)
                || (combinationResult.UpNumber >= 2 && combinationResult.RightNumber >= 2)
                || (combinationResult.DownNumber >= 2 && combinationResult.LeftNumber >= 2)
                || (combinationResult.DownNumber >= 2 && combinationResult.RightNumber >= 2);
        }
        public override bool TryProcess(Vector2Int startPosition, in SimilarCellsNumber combinationResult)
        {
            if (combinationResult.UpNumber >= 2 && combinationResult.LeftNumber >= 2)
            {
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.down, combinationResult.UpNumber);
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.left, combinationResult.LeftNumber);
                GameField.ExplodeCellAndReplace(startPosition, CellType.Bomb).Forget();

                return true;
            }
            else if (combinationResult.UpNumber >= 2 && combinationResult.RightNumber >= 2)
            {
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.down, combinationResult.UpNumber);
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.right, combinationResult.RightNumber);
                GameField.ExplodeCellAndReplace(startPosition, CellType.Bomb).Forget();

                return true;
            }
            else if (combinationResult.DownNumber >= 2 && combinationResult.RightNumber >= 2)
            {
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.up, combinationResult.DownNumber);
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.right, combinationResult.RightNumber);
                GameField.ExplodeCellAndReplace(startPosition, CellType.Bomb).Forget();

                return true;
            }
            else if (combinationResult.DownNumber >= 2 && combinationResult.LeftNumber >= 2)
            {
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.up, combinationResult.DownNumber);
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.left, combinationResult.LeftNumber);
                GameField.ExplodeCellAndReplace(startPosition, CellType.Bomb).Forget();

                return true;
            }

            return false;
        }
    }
}
