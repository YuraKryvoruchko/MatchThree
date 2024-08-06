using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Core.Infrastructure.Loading
{
    public interface ILoadingScreenProvider
    {
        UniTask LoadAndDestroy(ILoadingOperation operation);
        UniTask LoadAndDestroy(Queue<ILoadingOperation> operations);
    }
}
