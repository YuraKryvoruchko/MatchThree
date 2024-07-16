using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Core.Gameplay
{
    public class SuperAbility : IAbility
    {
        private GameField _gameField;

        void IAbility.Init(GameField gameField)
        {
            _gameField = gameField;
        }
        async UniTask IAbility.Execute(int xPosition, int yPosition)
        {
            List<Cell> cellList = _gameField.GetAllOfType(_gameField.GetCell(xPosition, yPosition).Type);
            UniTask[] tasks = new UniTask[cellList.Count];
            for (int i = 0; i < cellList.Count; i++)
            {
                if(!cellList[i].IsExplode)
                    tasks[i] = _gameField.ExplodeCell(cellList[i]);
            }
            await UniTask.WhenAll(tasks);
        }
    }
}
