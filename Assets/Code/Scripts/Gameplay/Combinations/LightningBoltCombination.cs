using UnityEngine;

using SimilarCellsNumber = Core.Gameplay.GameField.SimilarCellsNumber;

namespace Core.Gameplay
{
    public class LightningBoltCombination : CombinationBase
    {
        public LightningBoltCombination(GameField gameField, int score) : base(gameField, score)
        { }

        public override bool CanProcessedCombination(in SimilarCellsNumber combinationResult)
        {
            return (combinationResult.RightNumber >= 1 && combinationResult.UpNumber >= 1 && combinationResult.RightUpNumber >= 1)
                || (combinationResult.RightNumber >= 1 && combinationResult.DownNumber >= 1 && combinationResult.RightDownNumber >= 1)
                || (combinationResult.LeftNumber >= 1 && combinationResult.UpNumber >= 1 && combinationResult.LeftUpNumber >= 1)
                || (combinationResult.LeftNumber >= 1 && combinationResult.DownNumber >= 1 && combinationResult.LeftDownNumber >= 1);
        }
        public override bool TryProcess(Vector2Int startPosition, in SimilarCellsNumber combinationResult)
        {
            if (combinationResult.RightNumber >= 1 && combinationResult.UpNumber >= 1 && combinationResult.RightUpNumber >= 1)
            {
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.right, 1);
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.down, 1);
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.down + Vector2Int.right, 1);
                GameField.ExplodeCellAndReplace(startPosition, CellType.LightningBolt).Forget();

                return true;
            }
            else if (combinationResult.RightNumber >= 1 && combinationResult.DownNumber >= 1 && combinationResult.RightDownNumber >= 1)
            {
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.right, 1);
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.up, 1);
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.up + Vector2Int.right, 1);
                GameField.ExplodeCellAndReplace(startPosition, CellType.LightningBolt).Forget();

                return true;
            }
            else if (combinationResult.LeftNumber >= 1 && combinationResult.UpNumber >= 1 && combinationResult.LeftUpNumber >= 1)
            {
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.left, 1);
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.down, 1);
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.down + Vector2Int.left, 1);
                GameField.ExplodeCellAndReplace(startPosition, CellType.LightningBolt).Forget();

                return true;
            }
            else if (combinationResult.LeftNumber >= 1 && combinationResult.DownNumber >= 1 && combinationResult.LeftDownNumber >= 1)
            {
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.left, 1);
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.up, 1);
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.up + Vector2Int.left, 1);
                GameField.ExplodeCellAndReplace(startPosition, CellType.LightningBolt).Forget();

                return true;
            }

            return false;
        }
    }
}
