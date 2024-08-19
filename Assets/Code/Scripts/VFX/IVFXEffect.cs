#if UNITY_EDITOR
#endif

namespace Core.VFX.Abilities
{
    public interface IVFXEffect<T> : IBasicVFXEffect
    {
        void SetParameters(T parameters);
    }
}
