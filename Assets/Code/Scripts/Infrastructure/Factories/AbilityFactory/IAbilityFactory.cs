using Core.Gameplay;

namespace Core.Infrastructure.Factories
{
    public interface IAbilityFactory
    {
        IAbility GetAbility(CellType type);
        IAbility GetAdvancedAbility(CellType firstType, CellType secondType);
    }
}
