using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Cysharp.Threading.Tasks;
using Core.Gameplay;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Service;

namespace Core.UI.Gameplay
{
    public class UIAbilityHolder : MonoBehaviour
    {
        [SerializeField] private AbilityHolderButton[] _buttons;
        [SerializeField] private HorizontalLayoutGroup _layoutGroup;
        [Header("Audio Keys")]
        [SerializeField] private ClipEvent _uiClickKey;

        private IReadOnlyCollection<HolderAbilitySettings> _settings;

        private IAudioService _audioService;
        private AbilityThrowMode _abilityThrowMode;

        private AbilityHolderButton _clickedButton;

        [Inject] 
        private void Construct(IAudioService audioService, AbilityThrowMode abilityThrowMode)
        {
            _audioService = audioService;
            _abilityThrowMode = abilityThrowMode;
        }

        private void Start()
        {
            _abilityThrowMode.OnUse += OnUseAbility;
            _settings = _abilityThrowMode.GetHolderAbilitySettings();
            int buttonIndex = 0;
            foreach (HolderAbilitySettings abilitySettings in _settings)
            {
                InitButton(_buttons[buttonIndex], abilitySettings).Forget();
                buttonIndex++;
            }
            UniTask.Void(async () =>
            {
                await UniTask.Yield();
                _layoutGroup.enabled = false;
            });
        }
        private void OnDestroy()
        {
            _abilityThrowMode.OnUse -= OnUseAbility;

            int buttonIndex = 0;
            foreach (HolderAbilitySettings abilitySettings in _settings)
            {
                _buttons[buttonIndex].OnClick -= HandleClick;
                abilitySettings.Icon.ReleaseAsset();
                buttonIndex++;
            }
        }

        public void SetInteractable(bool value)
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].Interactable = value;
            }
        }

        private async UniTaskVoid InitButton(AbilityHolderButton button, HolderAbilitySettings settings)
        {
            Sprite icon = await settings.Icon.GetOrLoad();
            button.Init(icon, settings.AbilityType, settings.Amount);
            button.OnClick += HandleClick;
            button.gameObject.SetActive(true);
        }
        private void HandleClick(AbilityHolderButton button)
        {
            if (!_abilityThrowMode.CanUseAbility(button.Type))
                return;

            _audioService.PlayOneShot(_uiClickKey);
            if (button == _clickedButton && _abilityThrowMode.IsActive)
            {
                _clickedButton = null;
                _abilityThrowMode.DisableAbilityThrowMode();
            }
            else
            {
                _clickedButton = button;
                if (_abilityThrowMode.IsActive)
                    _abilityThrowMode.ChangeAbility(button.Type);
                else
                    _abilityThrowMode.EnableAbilityhrowMode(button.Type);
            }
        }
        private void OnUseAbility(CellType type, int count)
        {
            _clickedButton.SetAmount(count);
            _clickedButton = null;
        }
    }
}
