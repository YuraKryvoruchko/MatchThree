using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Cell : MonoBehaviour
{
    [SerializeField] private CellType _type;

    public CellType Type { get => _type; }
    public bool IsMove { get; private set; }

    public async void MoveTo(Vector3 endPosition, Action onComplete)
    {
        await MoveToWithTask(endPosition, onComplete);
    }
    public async UniTask MoveToWithTask(Vector3 endPosition, Action onComplete)
    {
        Vector3 startPosition = transform.position;

        float progress = 0;
        IsMove = true;
        while (progress < 1)
        {
            progress += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, endPosition, progress);
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        transform.position = endPosition;
        IsMove = false;
        onComplete?.Invoke();
    }
}
