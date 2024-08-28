using System;
using System.Collections.Generic;
using Zenject;

using CellExplosionResult = Core.Gameplay.GameField.CellExplosionResult;

namespace Core.Gameplay
{
    public class LevelTaskCompletionChecker : IInitializable, IDisposable
    {
        private LevelTask[] _levelTasks;
        private GameField _gameField;

        private Dictionary<CellType, int> _dictionary;

        public event Action<CellType, int> OnExplodeCell;
        public event Action OnAllTaskCompleted;

        public LevelTaskCompletionChecker(GameField gameField)
        {
            _dictionary = new Dictionary<CellType, int>();
            _gameField = gameField;
        }
        public void Initialize()
        {
            _gameField.OnExplodeCellWithResult += HandleCellExplosion;
        }
        public void Dispose()
        {
            _gameField.OnExplodeCellWithResult -= HandleCellExplosion;
        }

        private void HandleCellExplosion(CellExplosionResult cellExplosionResult)
        {
            _dictionary[cellExplosionResult.Type]--;
            OnExplodeCell?.Invoke(cellExplosionResult.Type, _dictionary[cellExplosionResult.Type]);

            if (_dictionary[cellExplosionResult.Type] == 0)
                _dictionary.Remove(cellExplosionResult.Type);

            if (_dictionary.Count == 0)
                OnAllTaskCompleted?.Invoke();
        }
    }

    [Serializable]
    public struct LevelTask
    {
        public int Count;
        public CellType CellType;
    }
}
