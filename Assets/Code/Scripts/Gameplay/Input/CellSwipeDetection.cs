using System;
using UnityEngine;
using Zenject;
using Core.Input;

namespace Core.Gameplay.Input
{
    public class CellSwipeDetection : IInitializable, IDisposable
    {
        private Camera _mainCamera;
        private SwipeDetection _swipeDetection;

        private Vector2 _cellPosition;

        private const float RAY_DISTANCE = 100F;

        public event Action<Vector2, Vector2> OnTrySwipeCellWithGetDirection;

        public CellSwipeDetection(Camera mainCamera, SwipeDetection swipeDetection)
        {
            _mainCamera = mainCamera;
            _swipeDetection = swipeDetection;
        }

        void IInitializable.Initialize()
        {
            _swipeDetection.OnStartSwipe += GetCellFromPosition;
            _swipeDetection.OnSwipe += MoveCell;
        }
        void IDisposable.Dispose()
        {
            _swipeDetection.OnStartSwipe -= GetCellFromPosition;
            _swipeDetection.OnSwipe -= MoveCell;
        }

        private void GetCellFromPosition(Vector2 screenPosition)
        {
            Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.blue, 5f);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, RAY_DISTANCE);
            if (hit.collider == null)
                return;

            _cellPosition = hit.collider.transform.position;
        }
        private void MoveCell(Vector2 swipeDirection)
        {
            OnTrySwipeCellWithGetDirection?.Invoke(_cellPosition, swipeDirection);
        }
    }
}
