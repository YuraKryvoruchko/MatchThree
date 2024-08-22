using System;
using Core.Gameplay.Input;
using UnityEngine;

namespace Core.Gameplay
{
    public class AbilityThrowMode
    {
        private CellClickDetection _cellClickDetection;
        private GameField _gameField;

        private CellType _abilityType;

        public bool IsActive { get; private set; }

        public event Action OnEnableMode;
        public event Action OnDisableMode;

        public AbilityThrowMode(GameField gameField, CellClickDetection cellClickDetection)
        {
            _gameField = gameField;
            _cellClickDetection = cellClickDetection;
        }

        public void Handle(Cell cell)
        {
            Vector2Int cellPosition = _gameField.WorldPositionToCell(cell.transform.position);
            _gameField.UseAbility(_abilityType, cellPosition, cellPosition);
            DisableAbilityThrowMode();
        }
        public void EnableAbilityhrowMode(CellType abilityType)
        {
            _abilityType = abilityType;
            _cellClickDetection.OnCellClick += Handle;
            IsActive = true;
            OnEnableMode?.Invoke();
        }
        public void DisableAbilityThrowMode()
        {
            _cellClickDetection.OnCellClick -= Handle;
            IsActive = false;
            OnDisableMode?.Invoke();
        }
        public void ChangeAbility(CellType abilityType)
        {
            _abilityType = abilityType;
        }
    }
}
