using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

public class GameField : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private int _verticalMapSize = 5;
    [SerializeField] private int _horizontalMapSize = 5;
    [SerializeField] private Transform _startMapPoint;
    [SerializeField] private Transform _cellContainer;
    [SerializeField] private CellType[] _availableRandomCellTypes;
    [SerializeField] private CellConfig[] _cellConfigs;
    [Header("Cell Settings")]
    [SerializeField] private float _interval;
    [Header("Other Services")]
    [SerializeField] private FieldCellPool _cellPool;
    [SerializeField] private CellSwipeDetection _cellSwipeDetection;

    private Cell[,] _map;
    
    private bool _gameBlock = true;

    [Serializable]
    private class CellConfig
    {
        public int X, Y;
        public CellType Type; 
    }

    private void OnEnable()
    {
        _cellSwipeDetection.OnTrySwipeCellWithGetDirection += Handle;
    }
    private void OnDisable()
    {
        _cellSwipeDetection.OnTrySwipeCellWithGetDirection -= Handle;
    }
    private async void Start()
    {
        _cellPool.Init();
        _map = new Cell[_verticalMapSize, _horizontalMapSize];
        for(int i = 0; i < _cellConfigs.Length; i++)
        {
            _map[_cellConfigs[i].Y, _cellConfigs[i].X] = _cellPool.GetCell(_cellConfigs[i].Type,
                GetElementPosition(_cellConfigs[i].X, _cellConfigs[i].Y), Quaternion.identity, _cellContainer);
        }

        await MoveDownElements(_map);
        _gameBlock = false;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3[] lines = new Vector3[4];
        lines[0] = new Vector3(_startMapPoint.position.x, _startMapPoint.position.y, 0);
        lines[1] = new Vector3(_startMapPoint.position.x + _interval * (_horizontalMapSize - 1), 
            _startMapPoint.position.y, 0);
        lines[2] = new Vector3(_startMapPoint.position.x, 
            _startMapPoint.position.y - _interval * (_verticalMapSize - 1), 0);
        lines[3] = new Vector3(_startMapPoint.position.x + _interval * (_horizontalMapSize - 1), 
            _startMapPoint.position.y - _interval * (_verticalMapSize - 1), 0);
        Gizmos.DrawLine(lines[0], lines[1]);
        Gizmos.DrawLine(lines[1], lines[3]);
        Gizmos.DrawLine(lines[3], lines[2]);
        Gizmos.DrawLine(lines[2], lines[0]);
    }

    public async void Handle(Vector2 cellPosition, Vector2 swipeDirection)
    {
        if (_gameBlock)
            return;

        _gameBlock = true;

        swipeDirection = swipeDirection.normalized;
        int firstXPosition = Mathf.RoundToInt((cellPosition.x - _startMapPoint.position.x) / _interval);
        int firstYPosition = Mathf.RoundToInt((_startMapPoint.position.y - cellPosition.y) / _interval);
        int secondXPosition = firstXPosition + Mathf.RoundToInt(swipeDirection.x);
        int secondYPosition = firstYPosition - Mathf.RoundToInt(swipeDirection.y);
        if (_map[firstYPosition, firstXPosition].IsStatic || _map[secondYPosition, secondXPosition].IsStatic)
        {
            _gameBlock = false;
            return;
        }

        Cell tmpCell = _map[firstYPosition, firstXPosition];
        Vector3 tmpPosition = tmpCell.transform.position;
        UniTask firstMoveTask = _map[firstYPosition, firstXPosition].MoveToWithTask(_map[secondYPosition, secondXPosition].transform.position, false);
        UniTask secondMoveTask = _map[secondYPosition, secondXPosition].MoveToWithTask(tmpPosition, false);
        _map[firstYPosition, firstXPosition] = _map[secondYPosition, secondXPosition];
        _map[secondYPosition, secondXPosition] = tmpCell;

        await UniTask.WhenAll(firstMoveTask, secondMoveTask);

        bool isFirstElementMoved = HandleMove(firstXPosition, firstYPosition, _map);
        bool isSecondElementMoved = HandleMove(secondXPosition, secondYPosition, _map);

        if (!isFirstElementMoved && !isSecondElementMoved)
        {
            tmpCell = _map[firstYPosition, firstXPosition];
            tmpPosition = tmpCell.transform.position;
            firstMoveTask = _map[firstYPosition, firstXPosition].MoveToWithTask(_map[secondYPosition, secondXPosition].transform.position, false);
            secondMoveTask = _map[secondYPosition, secondXPosition].MoveToWithTask(tmpPosition, false);
            _map[firstYPosition, firstXPosition] = _map[secondYPosition, secondXPosition];
            _map[secondYPosition, secondXPosition] = tmpCell;

            await UniTask.WhenAll(firstMoveTask, secondMoveTask);
        }
        else
        {
            await MoveDownElements(_map);
        }

        _gameBlock = false;
    }

    private bool HandleMove(int xPosition, int yPosition, Cell[,] map)
    {
        if (map[yPosition, xPosition] == null)
            return false;

        int rightNumber = GetRightElementsNumber(xPosition, yPosition, map);
        int leftNumber = GetLeftElementsNumber(xPosition, yPosition, map);
        int upNumber = GetUpElementsNumber(xPosition, yPosition, map);
        int downNumber = GetDownElementsNumber(xPosition, yPosition, map);

        if (rightNumber + leftNumber >= 2)
            DeleteElements(xPosition, yPosition, rightNumber, leftNumber, 0, 0, 0, map);
        else if (upNumber + downNumber >= 2)
            DeleteElements(xPosition, yPosition, 0, 0, upNumber, downNumber, 0, map);
        else
            return false;

        return true;
    }

    private int GetRightElementsNumber(int xPosition, int yPosition, Cell[,] map)
    {
        if (xPosition + 1 >= _horizontalMapSize || map[yPosition, xPosition + 1] == null || map[yPosition, xPosition].Type != map[yPosition, xPosition + 1].Type)
            return 0;

        return 1 + GetRightElementsNumber(xPosition + 1, yPosition, map);
    }
    private int GetLeftElementsNumber(int xPosition, int yPosition, Cell[,] map)
    {
        if (xPosition - 1 < 0 || map[yPosition, xPosition - 1] == null || map[yPosition, xPosition].Type != map[yPosition, xPosition - 1].Type)
            return 0;

        return 1 + GetLeftElementsNumber(xPosition - 1, yPosition, map);
    }
    private int GetUpElementsNumber(int xPosition, int yPosition, Cell[,] map)
    {
        if (yPosition - 1 < 0 || map[yPosition - 1, xPosition] == null || map[yPosition, xPosition].Type != map[yPosition - 1, xPosition].Type)
            return 0;

        return 1 + GetUpElementsNumber(xPosition, yPosition - 1, map);
    }
    private int GetDownElementsNumber(int xPosition, int yPosition, Cell[,] map)
    {
        if (yPosition + 1 >= _verticalMapSize || map[yPosition + 1, xPosition] == null || map[yPosition, xPosition].Type != map[yPosition + 1, xPosition].Type)
            return 0;

        return 1 + GetDownElementsNumber(xPosition, yPosition + 1, map);
    }

    private void DeleteElements(int xPosition, int yPosition, int rightNumber, int leftNumber, int upNumber,
        int downNumber, CellType createdElement, Cell[,] map)
    {
        for (int i = 1; i <= rightNumber; i++)
        {
            _cellPool.ReturnCell(map[yPosition, xPosition + i]);
            map[yPosition, xPosition + i] = null;
        }
        for (int i = 1; i <= leftNumber; i++)
        {
            _cellPool.ReturnCell(map[yPosition, xPosition - i]);
            map[yPosition, xPosition - i] = null;
        }
        for (int i = 1; i <= upNumber; i++)
        {
            _cellPool.ReturnCell(map[yPosition - i, xPosition]);
            map[yPosition - i, xPosition] = null;
        }
        for (int i = 1; i <= downNumber; i++) 
        {
            _cellPool.ReturnCell(map[yPosition + i, xPosition]);
            map[yPosition + i, xPosition] = null;
        }

        if (createdElement == 0)
        {
            _cellPool.ReturnCell(map[yPosition, xPosition]);
            map[yPosition, xPosition] = null;
        }
        else
        {
            map[yPosition, xPosition] = _cellPool.GetCell(createdElement, map[yPosition, xPosition].transform.position, Quaternion.identity);
        }
    }

    private async UniTask MoveDownElements(Cell[,] map)
    {
        int x = 0, y = 0;
        bool areElementsMoved = false;
        do
        {
            areElementsMoved = true;
            for (int i = 0; i < _horizontalMapSize; i++)
            {
                int lowerElementIndex = 0, spawnQueue = 0;
                for (int j = _verticalMapSize - 1; j > 0; j--)
                {
                    if(map[j, i] == null)
                    {
                        areElementsMoved = false;
                        spawnQueue++;
                        if (lowerElementIndex < j)
                            lowerElementIndex = j;
                    }
                    else
                    {
                        if (map[j, i].IsStatic)
                            continue;

                        if (map[j, i].IsMove)
                        {
                            areElementsMoved = false;
                            continue;
                        }

                        if (lowerElementIndex <= j)
                            continue;

                        areElementsMoved = false;
                        map[lowerElementIndex, i] = map[j, i];
                        map[j, i] = null;
                        map[lowerElementIndex, i].MoveTo(GetElementPosition(i, lowerElementIndex), true, DoCallback);
                        lowerElementIndex--;
                    }
                }
                int upperElementIndex;
                for (upperElementIndex = 0; upperElementIndex < _verticalMapSize; upperElementIndex++)
                {
                    if (map[upperElementIndex, i] == null || !map[upperElementIndex, i].IsStatic)
                        break;
                }
                if (map[upperElementIndex, i] == null)
                {
                    Vector2 pos = GetElementPosition(i, upperElementIndex - spawnQueue - 1);
                    map[lowerElementIndex, i] = _cellPool.GetCell(GetRandomElementType(),
                        new Vector3(pos.x, pos.y, 0), Quaternion.identity, _cellContainer);
                    map[lowerElementIndex, i].MoveTo(GetElementPosition(i, lowerElementIndex), true, DoCallback);
                    areElementsMoved = false;
                }
                else
                {
                    if (map[upperElementIndex, i].IsMove)
                    {
                        areElementsMoved = false;
                        continue;
                    }
                    if (lowerElementIndex <= upperElementIndex)
                        continue;

                    areElementsMoved = false;
                    map[lowerElementIndex, i] = map[upperElementIndex, i];
                    map[upperElementIndex, i] = null;
                    map[lowerElementIndex, i].MoveTo(GetElementPosition(i, lowerElementIndex), true, DoCallback);
                    lowerElementIndex--;
                }
            }
            await UniTask.Yield();
        } while (!areElementsMoved);

        void DoCallback(Cell cell)
        {
            x = Mathf.RoundToInt((cell.transform.position.x - _startMapPoint.position.x) / _interval);
            y = Mathf.RoundToInt((_startMapPoint.position.y - cell.transform.position.y) / _interval);
            bool handled = HandleMove(x, y, map);
            if (handled == true)
                areElementsMoved = false;
        }
    }
    private CellType GetRandomElementType()
    {
        int index = UnityEngine.Random.Range(0, _availableRandomCellTypes.Length);
        if (index == _availableRandomCellTypes.Length)
            index--;

        return _availableRandomCellTypes[index];
    }
    private Vector2 GetElementPosition(int xIndex, int yIndex)
    {
        return new Vector2(_startMapPoint.position.x + _interval * xIndex, 
            _startMapPoint.position.y - _interval * yIndex);
    }
}
