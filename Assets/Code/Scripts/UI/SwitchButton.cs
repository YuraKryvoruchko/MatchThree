using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Core.UI
{
    public class SwitchButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [Header("Visual")]
        [SerializeField] private Transform _toggle;
        [SerializeField] private Transform _onPosition;
        [SerializeField] private Transform _offPosition;
        [SerializeField] private float _switchDelay = 0.1f;

        private bool _isActive;

        public Button Button { get => _button; }

        public void Switch()
        {
            _isActive = !_isActive;
            DoSwitchAnimation();
        }
        public void SetActive(bool active)
        {
            _isActive = active;
            if (_isActive)
                _toggle.localPosition = new Vector3(_onPosition.localPosition.x, _toggle.localPosition.y, _toggle.localPosition.z);
            else
                _toggle.localPosition = new Vector3(_offPosition.localPosition.x, _toggle.localPosition.y, _toggle.localPosition.z);
        }

        private void DoSwitchAnimation()
        {
            if (_isActive)
                _toggle.DOLocalMoveX(_onPosition.localPosition.x, _switchDelay);
            else
                _toggle.DOLocalMoveX(_offPosition.localPosition.x, _switchDelay);
        }
    }
}
