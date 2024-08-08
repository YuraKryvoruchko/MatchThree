using UnityEngine;
#if UNITY_EDITOR
#endif

namespace Core.VFX.Abilities
{
    public class LineRendererLerp : MonoBehaviour
    {
        [SerializeField] private LineRenderer _line;

        private Vector3 _startPosition;
        private Vector3 _endPosition;

        public void Init(Vector3 startPosition, Vector3 endPosition)
        {
            _startPosition = startPosition;
            _endPosition = endPosition;
        }
        public void Lerp(float progress)
        {
            _line.SetPosition(0, _startPosition);
            _line.SetPosition(1, Vector3.Lerp(_startPosition, _endPosition, progress));
        }
    }
}
