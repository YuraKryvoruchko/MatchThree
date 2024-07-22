using System;
using UnityEngine;
using UnityEngine.UI;
using Core.Infrastructure.UI;
using Core.Infrastructure.Service;

namespace Core.UI.Gameplay
{
    public class GameplayMenu : WindowBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _pauseMenu;
        [Header("Popups")]
        [SerializeField] private WindowBase _popupPrefab;

        private IWindowService _windowService;

        public override event Action OnMenuBack;

        private void Construct(IWindowService windowService)
        {
            _windowService = windowService;
        }

        protected override void OnShow()
        {
            _pauseMenu.onClick.AddListener(CreateMenuPopup);
        }
        protected override void OnHide()
        {
            _pauseMenu.onClick.RemoveListener(CreateMenuPopup);
        }
        protected override void OnFocus()
        {
            _pauseMenu.interactable = true;
        }
        protected override void OnUnfocus()
        {
            _pauseMenu.interactable = false;
        }
        protected override void OnClose()
        {
            OnHide();
            OnUnfocus();
        }

        private void CreateMenuPopup()
        {
            _windowService.OpenPopup<WindowBase>(_popupPrefab.Path);
        }
    }
}
