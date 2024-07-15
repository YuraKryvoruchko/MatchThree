using Core.Gameplay;

namespace Core.Infrastructure.Factories
{
    public interface IAbilityFactory
    {
        IAbility GetAbility(CellType type);
    }
}
