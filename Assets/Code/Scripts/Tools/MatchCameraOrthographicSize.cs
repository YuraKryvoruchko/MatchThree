using UnityEngine;

namespace Core.Tools
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class MatchCameraOrthographicSize : MonoBehaviour
    {
        [SerializeField] private float _sceneWidth = 9.45f;
        [SerializeField] private float _sceneHeight = 16.8f;

        private Camera _camera;

        void Start()
        {
            _camera = GetComponent<Camera>();
        }

        void Update()
        {
            float widthUnitsPerPixel = _sceneWidth / Screen.width;
            float heightUnitsPerPixel = _sceneHeight / Screen.height;

            float desiredHalfHeight;
            if (widthUnitsPerPixel < heightUnitsPerPixel)
                desiredHalfHeight = 0.5f * heightUnitsPerPixel * Screen.height;
            else
                desiredHalfHeight = 0.5f * widthUnitsPerPixel * Screen.height;

            _camera.orthographicSize = desiredHalfHeight;
        }
    }
}
