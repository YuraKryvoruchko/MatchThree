using System;
using System.Collections.Generic;
using Zenject;
using Core.Infrastructure.Service;

using CellExplosionResult = Core.Gameplay.GameField.CellExplosionResult;

namespace Core.Gameplay
{
    public class LevelTaskCompletionChecker : IInitializable, IDisposable
    {
        private GameField _gameField;

        private LevelTask[] _tasks;
        private Dictionary<CellType, int> _dictionary;

        private int _totalTaskElementCount = 0;
        private int _completedTaskElementCount = 0;

        public event Action<CellType, int> OnExplodeCell;
        public event Action OnAllTaskCompleted;

        public LevelTask[] Tasks { get => _tasks; }

        public LevelTaskCompletionChecker(GameField gameField, ILevelService levelService)
        {
            _gameField = gameField;
            _tasks = levelService.GetCurrentLevelConfig().Tasks;
            _dictionary = new Dictionary<CellType, int>(_tasks.Length);
            for (int i = 0; i < _tasks.Length; i++)
            {
                _dictionary.Add(_tasks[i].CellType, _tasks[i].Count);
                _totalTaskElementCount += _tasks[i].Count;
            }
        }
        public void Initialize()
        {
            _gameField.OnExplodeCellWithResult += HandleCellExplosion;
        }
        public void Dispose()
        {
            _dictionary.Clear();
            _gameField.OnExplodeCellWithResult -= HandleCellExplosion;
        }

        public float GetProgress()
        {
            return _completedTaskElementCount / _totalTaskElementCount;
        }

        private void HandleCellExplosion(CellExplosionResult cellExplosionResult)
        {
            if (!_dictionary.ContainsKey(cellExplosionResult.Type))
                return;

            _completedTaskElementCount++;
            _dictionary[cellExplosionResult.Type]--;
            OnExplodeCell?.Invoke(cellExplosionResult.Type, _dictionary[cellExplosionResult.Type]);

            if (_dictionary[cellExplosionResult.Type] == 0)
                _dictionary.Remove(cellExplosionResult.Type);

            if (_dictionary.Count == 0)
                OnAllTaskCompleted?.Invoke();
        }
    }
}
