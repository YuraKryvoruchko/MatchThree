using System;
using UnityEngine;
using Zenject;
using Core.Input;

namespace Core.Gameplay.Input
{
    public class BoardClickDetection : IInitializable, IDisposable
    {
        private Camera _mainCamera;
        private SwipeDetection _swipeDetection;

        private float RAY_DISTANCE = 100F;

        public event Action<Vector3> OnBoardClick;

        public BoardClickDetection(Camera mainCamera, SwipeDetection swipeDetection)
        {
            _mainCamera = mainCamera;
            _swipeDetection = swipeDetection;
        }

        void IInitializable.Initialize()
        {
            _swipeDetection.OnStartSwipe += GetPointFromPosition;
        }
        void IDisposable.Dispose()
        {
            _swipeDetection.OnStartSwipe -= GetPointFromPosition;
        }

        private void GetPointFromPosition(Vector2 screenPosition)
        {
            Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.blue, 5f);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, RAY_DISTANCE);
            if (hit.collider == null)
                return;

            OnBoardClick?.Invoke(hit.point);
        }
    }
}
