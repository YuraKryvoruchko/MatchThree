using UnityEngine;
using Cysharp.Threading.Tasks;
using Core.Infrastructure.UI;

namespace Core.Infrastructure.Factories
{
    public interface IWindowFactory
    {
        UniTask<T> GetWindow<T>(string key, Transform parent) where T : WindowBase;
        void ReleaseWindow(WindowBase window);
    }
}
