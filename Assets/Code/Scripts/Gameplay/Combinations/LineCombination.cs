using UnityEngine;
using Cysharp.Threading.Tasks;

using SimilarCellsNumber = Core.Gameplay.GameField.SimilarCellsNumber;

namespace Core.Gameplay
{
    public class LineCombination : CombinationBase
    {
        private int _minLength;

        public LineCombination(GameField gameField, int score, int minLength) : base(gameField, score)
        {
            _minLength = minLength - 1;
        }

        public override bool CanProcessedCombination(in SimilarCellsNumber combinationResult)
        {
            return (combinationResult.UpNumber + combinationResult.DownNumber >= _minLength)
                || (combinationResult.LeftNumber + combinationResult.RightNumber >= _minLength);
        }
        public override bool TryProcess(Vector2Int startPosition, in SimilarCellsNumber combinationResult)
        {
            if (combinationResult.UpNumber + combinationResult.DownNumber >= _minLength)
            {
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.down, combinationResult.UpNumber);
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.up, combinationResult.DownNumber);
                GameField.ExplodeCellAsync(startPosition).Forget();

                return true;
            }
            else if (combinationResult.LeftNumber + combinationResult.RightNumber >= _minLength)
            {
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.left, combinationResult.LeftNumber);
                GameField.ExplodeCellsOnDirection(startPosition, Vector2Int.right, combinationResult.RightNumber);
                GameField.ExplodeCellAsync(startPosition).Forget();

                return true;
            }

            return false;
        }
    }
}
