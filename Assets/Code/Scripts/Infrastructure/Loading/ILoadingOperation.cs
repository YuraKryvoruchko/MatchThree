using System;
using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.Loading
{
    public interface ILoadingOperation
    {
        public string Description { get; }

        public UniTask Load(Action<float> onProgress);
    }
}
