using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Infrastructure.UI
{
    public abstract class WindowBase : MonoBehaviour, IWindow
    {
        #region Fields

        [Header("Window Settings")]
        [SerializeField] private string _windowPrefabPath;
        [SerializeField] private bool _isPopup;
        [SerializeField] private Canvas _menuCanvas;

        private Stack<State> _stateStack = new Stack<State>();

        #endregion

        #region Properties

        public string Path { get => _windowPrefabPath; }
        public bool IsPopup { get => _isPopup; }

        public bool IsActive { get; private set; }
        public bool IsFocus { get; private set; }

        #endregion

        #region Actions

        public abstract event Action OnBack;
        public abstract event Action<WindowBase> OnClose;

        #endregion

        #region State

        private struct State
        {
            public bool IsActive;
            public bool IsFocus;
        }

        #endregion

        #region Public Methods

        public void Show()
        {
            IsActive = true;
            OnShow();
        }
        public void Hide()
        {
            IsActive = false;
            OnHide();
        }
        public void Focus()
        {
            IsFocus = true;
            OnFocus();
        }
        public void Unfocus()
        {
            IsFocus = false;
            OnUnfocus();
        }
        public void Push()
        {
            _stateStack.Push(new State() { IsActive = IsActive, IsFocus = IsFocus });
        }
        public void Back() 
        {
            if (_stateStack.Count == 0)
            {
                Close();
                return;
            }

            State state = _stateStack.Pop();
            if (state.IsActive != IsActive)
            {
                if (state.IsActive == true)
                    Show();
                else
                    Hide();
            }

            if (state.IsFocus != IsFocus)
            {
                if (state.IsFocus == true)
                    Focus();
                else
                    Unfocus();
            }
        }
        public void SetOrder(int order)
        {
            _menuCanvas.sortingOrder = order;
        }

        public abstract void Close();
        protected abstract void OnShow();
        protected abstract void OnHide();
        protected abstract void OnFocus();
        protected abstract void OnUnfocus();

        #endregion
    }
}
