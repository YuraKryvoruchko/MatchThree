using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.VFX
{
    public interface IBasicVFXEffect
    {
        UniTask Play(CancellationToken cancellationToken = default);
        void Pause(bool isPause);
        void Stop();

        event Action<IBasicVFXEffect> OnStart;
        event Action<IBasicVFXEffect, bool> OnPause;
        event Action<IBasicVFXEffect> OnComplete;
        event Action<IBasicVFXEffect> OnStopped;
    }
}
