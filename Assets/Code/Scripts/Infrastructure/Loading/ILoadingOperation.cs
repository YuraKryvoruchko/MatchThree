using System;
using Cysharp.Threading.Tasks;

namespace Core.Infrastructure.Loading
{
    public interface ILoadingOperation
    {
        public string Description { get; }

        public UniTask Load(Action<float> onProgress);
    }
}
