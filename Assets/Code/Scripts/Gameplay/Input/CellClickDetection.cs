using System;
using UnityEngine;
using Zenject;
using Core.Input;

namespace Core.Gameplay.Input
{
    public class CellClickDetection : IInitializable, IDisposable
    {
        private Camera _mainCamera;
        private SwipeDetection _swipeDetection;

        private float RAY_DISTANCE = 100F;

        public event Action<Cell> OnCellClick;

        public CellClickDetection(Camera mainCamera, SwipeDetection swipeDetection)
        {
            _mainCamera = mainCamera;
            _swipeDetection = swipeDetection;
        }

        void IInitializable.Initialize()
        {
            _swipeDetection.OnStartSwipe += GetCellFromPosition;
        }
        void IDisposable.Dispose()
        {
            _swipeDetection.OnStartSwipe -= GetCellFromPosition;
        }

        private void GetCellFromPosition(Vector2 screenPosition)
        {
            Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.blue, 5f);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, RAY_DISTANCE);
            if (hit.collider == null)
                return;

            OnCellClick?.Invoke(hit.collider.GetComponent<Cell>());
        }
    }
}
