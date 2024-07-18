using System;
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
        [SerializeField] private Button _ziperAbilityButton;
        [SerializeField] private Button _superAbilityButton;

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
            _ziperAbilityButton.onClick.AddListener(() => ActiveAbilityThrowMode(_abilityFactory.GetAbility(CellType.Zipper)));
            _superAbilityButton.onClick.AddListener(() => ActiveAbilityThrowMode(new SuperAbility()));
        }
        private void OnDestroy()
        {
            _bombAbilityButton.onClick.RemoveAllListeners();
            _ziperAbilityButton.onClick.RemoveAllListeners();
            _superAbilityButton.onClick.RemoveAllListeners();
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
