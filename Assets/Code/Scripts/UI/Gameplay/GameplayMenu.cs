using System;
using UnityEngine;
using UnityEngine.UI;
using Core.Infrastructure.UI;
using Core.Infrastructure.Service;
using Zenject;

namespace Core.UI.Gameplay
{
    public class GameplayMenu : WindowBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _pauseMenu;
        [Header("Modules")]
        [SerializeField] private UIAbilityHolder _uiAbilityHolder;
        [Header("Popups")]
        [SerializeField] private WindowBase _popupPrefab;

        private IWindowService _windowService;

        public override event Action OnMenuBack;

        [Inject]
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
            _uiAbilityHolder.SetInteractable(true);
        }
        protected override void OnUnfocus()
        {
            _pauseMenu.interactable = false;
            _uiAbilityHolder.SetInteractable(false);
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
