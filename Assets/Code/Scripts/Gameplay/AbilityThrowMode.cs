using Core.Gameplay.Input;

namespace Core.Gameplay
{
    public class AbilityThrowMode
    {
        private CellClickDetection _cellClickDetection;
        private GameField _gameField;

        private IAbility _ability;

        public bool IsActive { get; private set; }

        public AbilityThrowMode(GameField gameField, CellClickDetection cellClickDetection)
        {
            _gameField = gameField;
            _cellClickDetection = cellClickDetection;
        }

        public void Handle(Cell cell)
        {
            _gameField.UseAbility(_ability, cell.transform.position);
        }
        public void EnableAbilityhrowMode(IAbility ability)
        {
            _ability = ability;
            _cellClickDetection.OnCellClick += Handle;
            IsActive = true;
        }
        public void DisableAbilityThrowMode()
        {
            _ability = null;
            _cellClickDetection.OnCellClick -= Handle;
            IsActive = false;
        }
        public void ChangeAbility(IAbility ability)
        {
            _ability = ability;
        }
    }
}
