using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Core.Infrastructure.Factories;
using Core.Infrastructure.UI;

namespace Core.Infrastructure.Service
{
    public class WindowService : IWindowService
    {
        #region Fields

        private readonly IWindowFactory _windowFactory;

        private readonly Stack<WindowBase> _windowStack = new Stack<WindowBase>();

        private WindowBase _currentWindow;

        #endregion

        #region Constructor

        public WindowService(IWindowFactory windowFactory) 
        {
            _windowFactory = windowFactory;
        }

        #endregion

        #region Public Methods

        public async UniTask<T> OpenWindow<T>(string name) where T : WindowBase
        {
            T newWindow = await _windowFactory.GetWindow<T>(name, null);
            newWindow.OnStateStackEmpty += HandleWindowClosing;
            newWindow.OnMenuBack += Back;

            if(_currentWindow != null)
            {
                _currentWindow.Push();
                _currentWindow.Hide();
                _currentWindow.Unfocus();
            }

            WindowBase firstWindow = _windowStack.Count > 0 ? _windowStack.Peek() : null;
            if(firstWindow != null && firstWindow.IsPopup == true)
            {
                List<WindowBase> windows = GetPreviousOpenedWindows();
                Stack<WindowBase> openedPopups = GetOpenedPopups(windows);
                foreach (WindowBase openedPopup in openedPopups)
                {
                    openedPopup.Push();
                    openedPopup.Hide();
                    openedPopup.Unfocus();
                }
            }

            _windowStack.Push(newWindow);
            if(newWindow.IsPopup == false)
                _currentWindow = newWindow;
            newWindow.Show();
            newWindow.Focus();
            newWindow.SetOrder(_windowStack.Count);

            return newWindow;
        }
        public async UniTask<T> OpenPopup<T>(string name) where T : WindowBase
        {
            T popup = await _windowFactory.GetWindow<T>(name, null);
            popup.OnStateStackEmpty += HandleWindowClosing;
            popup.OnMenuBack += Back;

            WindowBase currentWindow = _windowStack.Peek();
            currentWindow.Push();
            if(currentWindow.IsPopup == true)
                currentWindow.Hide();
            currentWindow.Unfocus();

            _windowStack.Push(popup);
            popup.Show();
            popup.Focus();
            popup.SetOrder(_windowStack.Count);

            return popup;
        }
        public void Back()
        {
            if (_windowStack.Count == 0)
                return;

            WindowBase window = _windowStack.Pop();
            window.Back();
            OpenPreviousWindows();
        }
        public void BackToRoot()
        {
            while (_windowStack.Count > 1)
                Back();
        }

        #endregion

        #region Private Methods

        private void HandleWindowClosing(WindowBase window)
        {
            window.OnStateStackEmpty -= HandleWindowClosing;
            window.OnMenuBack -= Back;
            _windowFactory.ReleaseWindow(window);
        }
        private void OpenPreviousWindows()
        {
            if (_windowStack.Count == 0)
                return;

            List<WindowBase> openedWindows = GetPreviousOpenedWindows();
            Stack<WindowBase> openedPopups = GetOpenedPopups(openedWindows);
            WindowBase firstWindow = GetFirstWindow();
            bool isNoPopups = openedPopups.Count == 0;
            bool isOtherWindow = firstWindow != _currentWindow;

            if (isOtherWindow == true || isNoPopups == true)
            {
                firstWindow = openedWindows.Last();
                firstWindow.Back();
                _currentWindow = firstWindow;
            }

            if (isNoPopups == false)
            {
                WindowBase popup = openedPopups.Last();

                if (isOtherWindow == true)
                {
                    foreach (WindowBase openedPopup in openedPopups)
                        openedPopup.Back();
                }
                else
                {
                    popup.Back();
                }
            }
        }
        private List<WindowBase> GetPreviousOpenedWindows()
        {
            List<WindowBase> windows = new List<WindowBase>();

            bool hasWindow = false;
            foreach(WindowBase window in _windowStack)
            {
                if (hasWindow == true)
                    break;

                if (window.IsPopup == true)
                {
                    windows.Add(window);
                    continue;
                }

                windows.Add(window);
                hasWindow = true;
            }

            return windows;
        }
        private Stack<WindowBase> GetOpenedPopups(List<WindowBase> openedWindows)
        {
            Stack<WindowBase> openedPupups = new Stack<WindowBase>();

            foreach (WindowBase window in openedWindows)
            {
                if (window.IsPopup == false)
                    break;

                openedPupups.Push(window);
            }

            return openedPupups;
        }
        private WindowBase GetFirstWindow()
        {
            foreach(WindowBase window in _windowStack)
            {
                if (window.IsPopup == true)
                    continue;

                return window;
            }

            return null;
        }
        private void SetStateToWindow(WindowBase window, bool isActive, bool isFocus)
        {
            if (window.IsActive != isActive)
            {
                if (isActive == true)
                    window.Show();
                else
                    window.Hide();
            }
            if(window.IsFocus != isFocus)
            {
                if (isFocus == true)
                    window.Focus();
                else
                    window.Unfocus();
            }

            window.Push();
        }

        #endregion
    }
}
