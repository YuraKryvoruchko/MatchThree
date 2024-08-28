using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

using CellExplosionResult = Core.Gameplay.GameField.CellExplosionResult;

namespace Core.Gameplay
{
    public class LevelTaskCompletionChecker : IInitializable, IDisposable
    {
        private GameField _gameField;

        private LevelTask[] _tasks;
        private Dictionary<CellType, int> _dictionary;

        public event Action<CellType, int> OnExplodeCell;
        public event Action OnAllTaskCompleted;

        public LevelTask[] Tasks { get => _tasks; }

        public LevelTaskCompletionChecker(GameField gameField, LevelTask[] levelTasks)
        {
            _gameField = gameField;
            _tasks = levelTasks;
            _dictionary = new Dictionary<CellType, int>(levelTasks.Length);
            for (int i = 0; i < levelTasks.Length; i++)
                _dictionary.Add(levelTasks[i].CellType, levelTasks[i].Count);
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
    public class LevelTask
    {
        public Sprite Icon;
        public int Count;
        public CellType CellType;
    }
}
