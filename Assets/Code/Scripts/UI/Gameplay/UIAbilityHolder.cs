using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Core.Gameplay;

namespace Core.UI.Gameplay
{
    public class UIAbilityHolder : MonoBehaviour
    {
        [SerializeField] private Button _bombAbilityButton;
        [SerializeField] private Button _ziperAbilityButton;
        [SerializeField] private Button _superAbilityButton;

        private GameField _gameField;
        private AbilityThrowMode _abilityThrowMode;

        [Inject]
        private void Construct(GameField gameField, AbilityThrowMode abilityThrowMode)
        {
            _abilityThrowMode = abilityThrowMode;
            _gameField = gameField;
        }

        private void Start()
        {
            _bombAbilityButton.onClick.AddListener(() => ActiveAbilityThrowMode(new BombAbility()));
            _ziperAbilityButton.onClick.AddListener(() => ActiveAbilityThrowMode(new LightingBoltAbility(null)));
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
