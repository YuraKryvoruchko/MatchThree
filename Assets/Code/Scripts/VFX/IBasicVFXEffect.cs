using System;
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR
#endif

namespace Core.VFX.Abilities
{
    public interface IBasicVFXEffect
    {
        UniTask Play();
        void Pause(bool isPause);
        void Stop();

        event Action OnEnd;
    }
}
