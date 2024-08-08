using System;

namespace Core.Infrastructure.Service.Pause
{
    public class PauseService : IPauseService
    {
        private bool _isPause = false;

        bool IPauseService.IsPause { get => _isPause; }

        public event Action<bool> OnPause;

        void IPauseService.SetPause(bool isPause)
        {
            if (_isPause == isPause)
                return;

            _isPause = isPause;
            OnPause?.Invoke(isPause);
        }
    }
}
