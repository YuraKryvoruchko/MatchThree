using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Core.Gameplay;
using Core.Infrastructure.Factories;

namespace Core.UI.Gameplay
{
    public class UIAbilityHolder : MonoBehaviour
    {
        [SerializeField] private Button _bombAbilityButton;
        [SerializeField] private Button _lightningBoltAbilityButton;
        [SerializeField] private Button _supperAbilityButton;

        private IAbilityFactory _abilityFactory;
        private AbilityThrowMode _abilityThrowMode;

        [Inject]
        private void Construct(IAbilityFactory abilityFactory, AbilityThrowMode abilityThrowMode)
        {
            _abilityFactory = abilityFactory;
            _abilityThrowMode = abilityThrowMode;
        }

        private void Start()
        {
            _bombAbilityButton.onClick.AddListener(() => ActiveAbilityThrowMode(_abilityFactory.GetAbility(CellType.Bomb)));
            _lightningBoltAbilityButton.onClick.AddListener(() => ActiveAbilityThrowMode(_abilityFactory.GetAbility(CellType.LightningBolt)));
            _supperAbilityButton.onClick.AddListener(() => ActiveAbilityThrowMode(_abilityFactory.GetAbility(CellType.Supper)));
        }
        private void OnDestroy()
        {
            _bombAbilityButton.onClick.RemoveAllListeners();
            _lightningBoltAbilityButton.onClick.RemoveAllListeners();
            _supperAbilityButton.onClick.RemoveAllListeners();
        }
        private void ActiveAbilityThrowMode(IAbility ability)
        {
            if (_abilityThrowMode.IsActive)
                _abilityThrowMode.ChangeAbility(ability);
            else
                _abilityThrowMode.EnableAbilityhrowMode(ability);
        }
    }
}
