using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Core.Gameplay;
using Core.Infrastructure.Service.Audio;

namespace Core.UI.Gameplay
{
    public class UIAbilityHolder : MonoBehaviour
    {
        [SerializeField] private HolderAbilitySettings[] _settings;
        [SerializeField] private AbilityHolderButton[] _buttons;
        [SerializeField] private HorizontalLayoutGroup _layoutGroup;
        [Header("Audio Keys")]
        [SerializeField] private ClipEvent _uiClickKey;

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
            for (int i = 0; i < _settings.Length; i++)
            {
                HolderAbilitySettings settings = _settings[i];
                _buttons[i].Init(settings.Icon, settings.AbilityType, settings.Amount);
                _buttons[i].OnClick += HandleClick;
                _buttons[i].gameObject.SetActive(true);
            }
            Cysharp.Threading.Tasks.UniTask.Void(async () =>
            {
                await Cysharp.Threading.Tasks.UniTask.Yield();
                _layoutGroup.enabled = false;
            });
        }
        private void OnDestroy()
        {
            _abilityThrowMode.OnUse -= OnUseAbility;
            for (int i = 0; i < _settings.Length; i++)
            {
                _buttons[i].OnClick -= HandleClick;
            }
        }

        public void SetInteractable(bool value)
        {
            for (int i = 0; i < _settings.Length; i++)
            {
                _buttons[i].Interactable = value;
            }
        }

        private void HandleClick(AbilityHolderButton button)
        {
            if (button.Amount == 0)
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
        private void OnUseAbility()
        {
            _clickedButton.SetAmount(_clickedButton.Amount - 1);
            _clickedButton = null;
        }
    }
}
