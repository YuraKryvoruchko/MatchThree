using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Core.Gameplay;

namespace Core.UI.Gameplay
{
    public class PowerUpHolder : MonoBehaviour
    {
        [SerializeField] private Button _bombPowerUpButton;
        [SerializeField] private GameField _gameField;

        private void OnEnable()
        {
            _bombPowerUpButton.onClick.AddListener(() =>
            {
                _gameField.UsePowerUp(new BombPowerUp(), 4, 4);
            });
        }
        private void OnDisable()
        {
            _bombPowerUpButton.onClick.RemoveAllListeners();
        }
    }
}
