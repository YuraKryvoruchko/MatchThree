﻿using System;
using UnityEngine;

public class CellMover : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private SwipeDetection _swipeDetection;

    private float RAY_DISTANCE = 100F;

    private void OnEnable()
    {
        _swipeDetection.OnStartSwipe += GetCellFromPosition;
        _swipeDetection.OnSwipe += MoveCell;
    }

    private void GetCellFromPosition(Vector2 screenPosition)
    {
        Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.blue, 5f);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, RAY_DISTANCE);
        if (hit.collider == null)
            return;
    }
    private void MoveCell(Vector2 swipeDirection)
    {
        throw new NotImplementedException();
    }
}
