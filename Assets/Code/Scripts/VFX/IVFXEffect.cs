namespace Core.VFX
{
    public interface IVFXEffect<T> : IBasicVFXEffect
    {
        void SetParameters(T parameters);
    }
}
