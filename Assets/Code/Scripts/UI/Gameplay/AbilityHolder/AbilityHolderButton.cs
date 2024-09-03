using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Gameplay;

namespace Core.UI.Gameplay
{
    public class AbilityHolderButton : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private Button _button;

        public CellType Type { get; private set; }
        public int Amount { get; private set; }

        public bool Interactable { get => _button.interactable; set => _button.interactable = value; }

        public event Action<AbilityHolderButton> OnClick;

        private void Awake()
        {
            _button.onClick.AddListener(() => OnClick?.Invoke(this));
        }
        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }

        public void Init(Sprite icon, CellType type, int amount)
        {
            _iconImage.sprite = icon;
            Type = type;
            SetAmount(amount);
        }
        public void SetAmount(int amount)
        {
            Amount = amount;
            _amountText.text = amount.ToString();
        }
    }
}
