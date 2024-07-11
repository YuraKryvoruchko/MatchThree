using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Cell : MonoBehaviour
{
    [Header("Basic Settings")]
    [SerializeField] private CellType _type;
    [SerializeField] private float _moveSpeed = 3;
    [Header("Features")]
    [SerializeField] private bool _isStatic = false;

    public CellType Type { get => _type; }
    public bool IsMove { get; private set; }
    public bool IsStatic { get => _isStatic; private set => _isStatic = value; }

    public async void MoveTo(Vector3 endPosition, bool inLocal = true, Action<Cell> onComplete = null)
    {
        await MoveToWithTask(endPosition, inLocal, onComplete);
    }
    public async UniTask MoveToWithTask(Vector3 endPosition, bool inLocal = true, Action<Cell> onComplete = null)
    {
        Vector3 startPosition = Vector3.zero;
        if(inLocal)
            startPosition = transform.localPosition;
        else
            startPosition = transform.position;

        Vector3 delta = endPosition - startPosition;
        float progress = 0;
        IsMove = true;
        while (progress < 1)
        {
            progress += _moveSpeed * Time.deltaTime;
            if(inLocal)
                transform.localPosition = Vector3.Lerp(startPosition, endPosition, progress);
            else
                transform.position = Vector3.Lerp(startPosition, endPosition, progress);
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        if (inLocal)
            transform.localPosition = endPosition;
        else
            transform.position = endPosition;
        IsMove = false;
        onComplete?.Invoke(this);
    }
}
