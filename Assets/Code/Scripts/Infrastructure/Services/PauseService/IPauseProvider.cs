using System;

namespace Core.Infrastructure.Service.Pause
{
    public interface IPauseProvider
    {
        event Action<bool> OnPause;
    }
}
