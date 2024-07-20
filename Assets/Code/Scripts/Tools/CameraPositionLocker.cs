using UnityEngine;

namespace Core.Tools
{
    [ExecuteInEditMode]
    public class CameraPositionLocker : MonoBehaviour
    {
        [SerializeField] private Transform _downPoint;

        private Camera _camera;

        private void Start()
        {
            _camera = GetComponent<Camera>();
        }

        private void Update()
        {
            _camera.transform.position = _downPoint.position + Vector3.up * _camera.orthographicSize;
        }         
    }
}
