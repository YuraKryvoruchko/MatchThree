using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Infrastructure.UI
{
    public abstract class WindowBase : MonoBehaviour, IWindow
    {
        [Header("Window Settings")]
        [SerializeField] private string _windowPrefabPath;
        [SerializeField] private bool _isPopup;
        [SerializeField] private Canvas _menuCanvas;

        private Stack<State> _stateStack = new Stack<State>();

        public string Path { get => _windowPrefabPath; }
        public bool IsPopup { get => _isPopup; }
        public bool IsActive { get; private set; }
        public bool IsFocus { get; private set; }

        public event Action<WindowBase> OnStateStackEmpty;
        public abstract event Action OnMenuBack;

        private struct State
        {
            public bool IsActive;
            public bool IsFocus;
        }

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
                OnClose();
                OnStateStackEmpty?.Invoke(this);
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

        protected abstract void OnShow();
        protected abstract void OnHide();
        protected abstract void OnFocus();
        protected abstract void OnUnfocus();
        protected abstract void OnClose();
    }
}
