using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Core.Extensions
{
    public static class LineRendererExtensions
    {
        public static void MoveTo(this LineRenderer line, int index, Vector3 to, float speed)
        {
            MoveToAsync(line, index, to, speed).Forget();
        }
        public static async UniTask MoveToAsync(this LineRenderer line, int index, Vector3 to, float speed)
        {
            Vector3 startPoint = line.GetPosition(0);
            float progress = 0f;
            while (progress < 1f)
            {
                progress += Time.deltaTime * speed;
                line.SetPosition(index, Vector3.Lerp(startPoint, to, progress));
                await UniTask.Yield();
            }
            line.SetPosition(index, to);
        }
    }
}
