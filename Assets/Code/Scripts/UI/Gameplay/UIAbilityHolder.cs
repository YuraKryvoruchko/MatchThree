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
        }
        private void OnDestroy()
        {
            _bombAbilityButton.onClick.RemoveAllListeners();
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
