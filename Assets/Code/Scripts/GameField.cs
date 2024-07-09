using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

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
    [Header("UI")]
    [SerializeField] private InputField _firstXValue;
    [SerializeField] private InputField _firstYValue;
    [SerializeField] private InputField _secondXValue;
    [SerializeField] private InputField _secondYValue;
    [Header("Other Services")]
    [SerializeField] private FieldCellPool _cellPool;

    private Cell[,] _map;
    
    private bool _gameBlock = true;

    [Serializable]
    private class CellConfig
    {
        public int X, Y;
        public CellType Type; 
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

    public async void Handle()
    {
        if (_gameBlock)
            return;

        _gameBlock = true;

        int firstXPosition = Convert.ToInt32(_firstXValue.text);
        int firstYPosition = Convert.ToInt32(_firstYValue.text);
        int secondXPosition = Convert.ToInt32(_secondXValue.text);
        int secondYPosition = Convert.ToInt32(_secondYValue.text);
        if (_map[firstYPosition, firstXPosition].IsStatic || _map[secondYPosition, secondXPosition].IsStatic)
        {
            _gameBlock = true;
            return;
        }

        Cell tmpCell = _map[firstYPosition, firstXPosition];
        Vector3 tmpPosition = tmpCell.transform.position;
        UniTask firstMoveTask = _map[firstYPosition, firstXPosition].MoveToWithTask(_map[secondYPosition, secondXPosition].transform.position);
        UniTask secondMoveTask = _map[secondYPosition, secondXPosition].MoveToWithTask(tmpPosition);
        _map[firstYPosition, firstXPosition] = _map[secondYPosition, secondXPosition];
        _map[secondYPosition, secondXPosition] = tmpCell;

        await UniTask.WhenAll(firstMoveTask, secondMoveTask);

        bool isFirstElementMoved = HandleMove(firstXPosition, firstYPosition, _map);
        bool isSecondElementMoved = HandleMove(secondXPosition, secondYPosition, _map);

        if (!isFirstElementMoved && !isSecondElementMoved)
        {
            tmpCell = _map[firstYPosition, firstXPosition];
            tmpPosition = tmpCell.transform.position;
            firstMoveTask = _map[firstYPosition, firstXPosition].MoveToWithTask(_map[secondYPosition, secondXPosition].transform.position);
            secondMoveTask = _map[secondYPosition, secondXPosition].MoveToWithTask(tmpPosition);
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
        bool areElementsMoved = false, areElementsHandled = false;
        bool[,] needHandle = new bool[_verticalMapSize, _horizontalMapSize];
        while (areElementsMoved == false || areElementsHandled == false)
        {
            areElementsMoved = true;
            List<UniTask> moveTasks = new List<UniTask>();
            for (int i = 0; i < _horizontalMapSize; i++)
            {
                for (int j = _verticalMapSize - 1; j > 0; j--)
                {
                    if (map[j, i] != null)
                        continue;
                    if (map[j - 1, i] != null && map[j - 1, i].IsStatic)
                        continue;

                    areElementsMoved = false;

                    map[j, i] = map[j - 1, i];
                    map[j - 1, i] = null;
                    if (map[j, i] != null)
                    {
                        moveTasks.Add(map[j, i].MoveToWithTask(map[j, i].transform.position + Vector3.down * _interval));
                        needHandle[j, i] = true;
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
                    map[upperElementIndex, i] = _cellPool.GetCell(GetRandomElementType(), 
                        GetElementPosition(i, upperElementIndex - 1), Quaternion.identity, _cellContainer);
                    moveTasks.Add(map[upperElementIndex, i].MoveToWithTask(GetElementPosition(i, upperElementIndex)));
                    needHandle[upperElementIndex, i] = true;
                }
            }
            await UniTask.WhenAll(moveTasks);
            areElementsHandled = true;
            for (int i = _verticalMapSize - 1; i >= 0; i--)
            {
                for (int j = 0; j < _horizontalMapSize; j++)
                {
                    if (needHandle[i, j] == false)
                        continue;

                    if (HandleMove(j, i, map) == true)
                        areElementsHandled = false;
                    else
                        needHandle[i, j] = false;
                }
            }
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
