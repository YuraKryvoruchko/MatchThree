using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Core.Gameplay;

namespace Core.UI.Gameplay
{
    public class AbilityHolder : MonoBehaviour
    {
        [SerializeField] private Button _bombAbilityButton;
        [SerializeField] private GameField _gameField;

        private void OnEnable()
        {
            _bombAbilityButton.onClick.AddListener(() =>
            {
                _gameField.UseAbility(new BombAbility(), 4, 4);
            });
        }
        private void OnDisable()
        {
            _bombAbilityButton.onClick.RemoveAllListeners();
        }
    }
}
