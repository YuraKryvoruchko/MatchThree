using UnityEngine;
using Cysharp.Threading.Tasks;
using com.cyborgAssets.inspectorButtonPro;

namespace Core.VFX.Abilities
{
    public class SupperAbilityEffect : MonoBehaviour
    {
        [SerializeField] private LineRenderer _linePrefab;
        [SerializeField] private AudioSource _audioSource;

        [ProPlayButton]
        private void Test(Vector3 point1, Vector3 point2)
        {
            Play(new Vector3[] { point1, point2 });
        }

        public void Play(Vector3[] endPositions)
        {
            _audioSource.Play();
            for(int i = 0; i < endPositions.Length; i++)
            {
                LineRenderer line = Instantiate(_linePrefab, this.transform);
                line.SetPosition(0, transform.position);
                line.MoveTo(1, endPositions[i], 0.25f);
            }
        }

        public void Pause(bool pause)
        {
            
        }
    }
    public static class LineRendererExtensions
    {
        public static async void MoveTo(this LineRenderer line, int index, Vector3 to, float speed)
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
