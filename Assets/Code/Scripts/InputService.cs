using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputService : MonoBehaviour
{
    [SerializeField] private float _swipeResistance;
    [SerializeField] private InputAction _positionAction;
    [SerializeField] private InputAction _pressAction;

    private Vector2 _initialPosition;

    private Vector2 CurrentPosition { get => _positionAction.ReadValue<Vector2>(); }

    public event Action<Vector2> OnSwipe;

    private void OnEnable()
    {
        _positionAction.Enable();
        _pressAction.Enable();
        _pressAction.performed += _ => { _initialPosition = CurrentPosition; };
        _pressAction.canceled += _ => DetectSwipe();
    }
    private void OnDisable()
    {
        _positionAction.Disable();
        _pressAction.Disable();
    }

    private void DetectSwipe()
    {
        Vector2 delta = CurrentPosition - _initialPosition;
        Vector2 direction = Vector2.zero;

        if (Mathf.Abs(delta.x) > _swipeResistance)
            direction.x = delta.x;
        if (Mathf.Abs(delta.y) > _swipeResistance)
            direction.y = delta.y;
        if(direction != Vector2.zero)
            OnSwipe?.Invoke(direction);
    }
}
