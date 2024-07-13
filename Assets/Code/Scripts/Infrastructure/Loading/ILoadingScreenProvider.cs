using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.Loading
{
    public interface ILoadingScreenProvider
    {
        UniTask LoadAndDestroy(ILoadingOperation operation);
        UniTask LoadAndDestroy(Queue<ILoadingOperation> operations);
    }
}
