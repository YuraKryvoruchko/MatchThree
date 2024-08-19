using System;
using Cysharp.Threading.Tasks;

namespace Core.VFX
{
    public interface IBasicVFXEffect
    {
        UniTask Play();
        void Pause(bool isPause);
        void Stop();

        event Action<IBasicVFXEffect> OnStart;
        event Action<IBasicVFXEffect, bool> OnPause;
        event Action<IBasicVFXEffect> OnComplete;
        event Action<IBasicVFXEffect> OnStoped;
    }
}
