using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Core.Gameplay;
using Core.Infrastructure.Service;
using Core.Infrastructure.Factories;

namespace Core.UI.Gameplay
{
    public class UIAbilityHolder : MonoBehaviour
    {
        [SerializeField] private Button _bombAbilityButton;
        [SerializeField] private Button _lightningBoltAbilityButton;
        [SerializeField] private Button _supperAbilityButton;

        private IAbilityFactory _abilityFactory;
        private UIAudioService _uiAudioService;
        private AbilityThrowMode _abilityThrowMode;

        private Button _clickedButton;

        [Inject] 
        private void Construct(IAbilityFactory abilityFactory, UIAudioService uiAudioService, AbilityThrowMode abilityThrowMode)
        {
            _abilityFactory = abilityFactory;
            _uiAudioService = uiAudioService;
            _abilityThrowMode = abilityThrowMode;
        }

        private void Start()
        {
            _bombAbilityButton.onClick.AddListener(() => HandleButtonClick(_bombAbilityButton, CellType.Bomb));
            _lightningBoltAbilityButton.onClick.AddListener(() => HandleButtonClick(_lightningBoltAbilityButton, CellType.LightningBolt));
            _supperAbilityButton.onClick.AddListener(() => HandleButtonClick(_supperAbilityButton, CellType.Supper));
        }
        private void OnDestroy()
        {
            _bombAbilityButton.onClick.RemoveAllListeners();
            _lightningBoltAbilityButton.onClick.RemoveAllListeners();
            _supperAbilityButton.onClick.RemoveAllListeners();
        }

        public void SetInteractable(bool value)
        {
            _bombAbilityButton.interactable = value;
            _lightningBoltAbilityButton.interactable = value;
            _supperAbilityButton.interactable = value;
        }

        private void HandleButtonClick(Button button, CellType type)
        {
            _uiAudioService.PlaySound(UISoundType.Click);
            if (button == _clickedButton && _abilityThrowMode.IsActive)
            {
                _clickedButton = null;
                _abilityThrowMode.DisableAbilityThrowMode();
            }
            else
            {
                _clickedButton = button;

                IAbility ability = _abilityFactory.GetAbility(type);
                if (_abilityThrowMode.IsActive)
                    _abilityThrowMode.ChangeAbility(ability);
                else
                    _abilityThrowMode.EnableAbilityhrowMode(ability);
            }
        }
    }
}
