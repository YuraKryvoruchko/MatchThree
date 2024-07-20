using UnityEngine;
using UnityEngine.UI;
using Zenject;
using DG.Tweening;
using Core.Gameplay;

namespace Core.UI.Gameplay
{
    public class UIAbilityThrowModeDisplay : MonoBehaviour
    {
        [Header("Vingnette Settings")]
        [SerializeField] private Color _vingnetteColor;
        [SerializeField] private float _vingnetteTimeDelay;
        [Header("Components")]
        [SerializeField] private Image _vingnetteImage;

        private AbilityThrowMode _abilityThrowMode;

        [Inject]
        private void Construct(AbilityThrowMode abilityThrowMode)
        {
            _abilityThrowMode = abilityThrowMode;
            _abilityThrowMode.OnEnableMode += Open;
            _abilityThrowMode.OnDisableMode += Close;
        }

        private void OnDestroy()
        {
            _abilityThrowMode.OnEnableMode -= Open;
            _abilityThrowMode.OnDisableMode -= Close;
        }

        public void Open()
        {
            _vingnetteImage.DOColor(_vingnetteColor, _vingnetteTimeDelay);
        }
        public void Close()
        {
            Color color = _vingnetteImage.color;
            color.a = 0;
            _vingnetteImage.DOColor(color, _vingnetteTimeDelay);
        }
    }
}
