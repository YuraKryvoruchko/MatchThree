using Cysharp.Threading.Tasks;
using Core.Infrastructure.UI;

namespace Core.Infrastructure.Service
{
    public interface IWindowService
    {
        UniTask<T> OpenWindow<T>(string name) where T : WindowBase;
        UniTask<T> OpenPopup<T>(string name) where T : WindowBase;
    }
}
