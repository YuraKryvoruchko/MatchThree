using UnityEngine;
using Core.Gameplay.Input;

namespace Core.Gameplay { 
    public class AbilityThrowMode
    {
        private CellSwipeDetection _cellSwipeDetection;
        private GameField _gameField;

        private IAbility _ability;

        public AbilityThrowMode(GameField gameField, CellSwipeDetection cellSwipeDetection)
        {
            _gameField = gameField;
            _cellSwipeDetection = cellSwipeDetection;
        }

        public void Handle(Vector2 cellPosition, Vector2 swipeDirection)
        {
            _gameField.UseAbility(_ability, (int)cellPosition.x, (int)cellPosition.y);
        }
        public void EnableAbilityhrowMode(IAbility ability)
        {
            _ability = ability;
            _cellSwipeDetection.OnTrySwipeCellWithGetDirection += Handle;
        }
        public void DisableAbilityThrowMode()
        {
            _ability = null;
            _cellSwipeDetection.OnTrySwipeCellWithGetDirection -= Handle;
        }
    }
}
