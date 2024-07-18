using Cysharp.Threading.Tasks;
using Core.VFX.Abilities;

namespace Core.Gameplay
{
    public class LightingBoltAbility : IAbility
    {
        private GameField _gameField;
        private LightingBoltEffect _lightingBoltEffect;

        public LightingBoltAbility(LightingBoltEffect lightingBoltEffect)
        {
            _lightingBoltEffect = lightingBoltEffect;
        }

        void IAbility.Init(GameField gameField)
        {
            _gameField = gameField;
        }
        async UniTask IAbility.Execute(int xPosition, int yPosition)
        {
            await _gameField.ExplodeCell(xPosition, yPosition);
        }
    }
}
