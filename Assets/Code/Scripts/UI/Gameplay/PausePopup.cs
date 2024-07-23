using System;
using UnityEngine;
using UnityEngine.UI;
using Core.Infrastructure.UI;

namespace Core.UI.Gameplay
{
    public class PausePopup : WindowBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _replayButton;
        [SerializeField] private Button _quitButton;

        public override event Action OnMenuBack;

        protected override void OnShow()
        {
            _closeButton.onClick.AddListener(BackMenu);
        }
        protected override void OnHide()
        {
            _closeButton.onClick.RemoveListener(BackMenu);
        }
        protected override void OnFocus()
        {
            _closeButton.interactable = true;
        }
        protected override void OnUnfocus()
        {
            _closeButton.interactable = false;
        }
        protected override void OnClose()
        {
            OnHide();
            OnUnfocus();
        }

        private void BackMenu()
        {
            OnMenuBack?.Invoke();
        }
    }
}
