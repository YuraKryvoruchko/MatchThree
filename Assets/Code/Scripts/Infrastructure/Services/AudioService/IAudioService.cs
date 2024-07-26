using System;

namespace Core.Infrastructure.Service
{
    public interface IAudioService<T> where T : Enum
    {
        float Volume { get; set; }

        void Play(T type);
    }
}
