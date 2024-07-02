using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameField : MonoBehaviour
{
    [SerializeField] private List<Cell> _cellPrefabs;
    [SerializeField] private float _interval;
    [Header("UI")]
    [SerializeField] private InputField _firstXValue;
    [SerializeField] private InputField _firstYValue;
    [SerializeField] private InputField _secondXValue;
    [SerializeField] private InputField _secondYValue;

    private Cell[,] _map;

    private bool _gameStarted = false;

    private const int VERTICAL_MAP_SIZE = 5;
    private const int HORIZONTAL_MAP_SIZE = 5;

    private const int BIG_BOMB = 9;
    private const int BOMB = 8;
    private const int HORIZONTAL_ROCKET = 7;
    private const int VERTICAL_ROCKET = 6;
    private const int PAPER = 5;

    private async void Start()
    {
        _map = new Cell[VERTICAL_MAP_SIZE, HORIZONTAL_MAP_SIZE];
        await MoveDownElements(_map);
        _gameStarted = true;
    }
    public async void Handle()
    {
        if (!_gameStarted)
            return;

        int firstXPosition = Convert.ToInt32(_firstXValue.text);
        int firstYPosition = Convert.ToInt32(_firstYValue.text);
        int secondXPosition = Convert.ToInt32(_secondXValue.text);
        int secondYPosition = Convert.ToInt32(_secondYValue.text);

        Cell tmpCell = _map[firstYPosition, firstXPosition];
        Vector3 tmpPosition = tmpCell.transform.position;
        _map[firstYPosition, firstXPosition].transform.position = _map[secondYPosition, secondXPosition].transform.position;
        _map[secondYPosition, secondXPosition].transform.position = tmpPosition;
        _map[firstYPosition, firstXPosition] = _map[secondYPosition, secondXPosition];
        _map[secondYPosition, secondXPosition] = tmpCell;

        await Task.Delay(250);

        bool isFirstElementMoved = HandleMove(firstXPosition, firstYPosition, _map);
        bool isSecondElementMoved = HandleMove(secondXPosition, secondYPosition, _map);

        if (isFirstElementMoved && isSecondElementMoved)
        {
            tmpCell = _map[firstYPosition, firstXPosition];
            tmpPosition = tmpCell.transform.position;
            _map[firstYPosition, firstXPosition].transform.position = _map[secondYPosition, secondXPosition].transform.position;
            _map[secondYPosition, secondXPosition].transform.position = tmpPosition;
            _map[firstYPosition, firstXPosition] = _map[secondYPosition, secondXPosition];
            _map[secondYPosition, secondXPosition] = tmpCell;
        }
        else
        {
            await MoveDownElements(_map);
        }
    }

    private bool HandleMove(int xPosition, int yPosition, Cell[,] map)
    {
        if (map[yPosition, xPosition] == null)
            return false;

        int rightNumber = GetRightElementsNumber(xPosition, yPosition, map);
        int leftNumber = GetLeftElementsNumber(xPosition, yPosition, map);
        int upNumber = GetUpElementsNumber(xPosition, yPosition, map);
        int downNumber = GetDownElementsNumber(xPosition, yPosition, map);
        /*int rightUpNumber = GetUpRightElementsNumber(xPosition, yPosition, map);
        //int leftUpNumber = GetUpLeftElementsNumber(xPosition, yPosition, map);
        //int rightDownNumber = GetDownRightElementsNumber(xPosition, yPosition, map);
        //int leftDownNumber = GetDownLeftElementsNumber(xPosition, yPosition, map);

        if (rightNumber + leftNumber >= 4)
            DeleteElements(xPosition, yPosition, rightNumber, leftNumber, 0, 0, 0, 0, 0, 0, BIG_BOMB, map);
        else if (upNumber + downNumber >= 4)
            DeleteElements(xPosition, yPosition, 0, 0, upNumber, downNumber, 0, 0, 0, 0, BIG_BOMB, map);
        else if (upNumber + leftNumber >= 4)
            DeleteElements(xPosition, yPosition, 0, leftNumber, upNumber, 0, 0, 0, 0, 0, BOMB, map);
        else if (upNumber + rightNumber >= 4)
            DeleteElements(xPosition, yPosition, rightNumber, 0, upNumber, 0, 0, 0, 0, 0, BOMB, map);
        else if (downNumber + leftNumber >= 4)
            DeleteElements(xPosition, yPosition, 0, leftNumber, 0, downNumber, 0, 0, 0, 0, BOMB, map);
        else if (downNumber + rightNumber >= 4)
            DeleteElements(xPosition, yPosition, rightNumber, 0, 0, downNumber, 0, 0, 0, 0, BOMB, map);
        else if (rightNumber + leftNumber >= 3)
            DeleteElements(xPosition, yPosition, rightNumber, leftNumber, 0, 0, 0, 0, 0, 0, HORIZONTAL_ROCKET, map);
        else if (upNumber + downNumber >= 3)
            DeleteElements(xPosition, yPosition, 0, 0, upNumber, downNumber, 0, 0, 0, 0, VERTICAL_ROCKET, map);
        else if (upNumber + leftNumber + leftUpNumber >= 3)
            DeleteElements(xPosition, yPosition, 0, leftNumber, upNumber, 0, 0, leftUpNumber, 0, 0, PAPER, map);
        else if (upNumber + rightNumber + rightUpNumber >= 3 && rightUpNumber == 1)
            DeleteElements(xPosition, yPosition, rightNumber, 0, upNumber, 0, rightUpNumber, 0, 0, 0, PAPER, map);
        else if (downNumber + rightNumber + rightDownNumber >= 3 && rightDownNumber == 1)
            DeleteElements(xPosition, yPosition, rightNumber, 0, 0, downNumber, 0, 0, rightDownNumber, 0, PAPER, map);
        else if (downNumber + leftNumber + leftDownNumber >= 3 && leftDownNumber == 1)
            DeleteElements(xPosition, yPosition, 0, leftNumber, 0, downNumber, 0, 0, 0, leftDownNumber, PAPER, map);
        else*/ 
        if (rightNumber + leftNumber >= 2)
            DeleteElements(xPosition, yPosition, rightNumber, leftNumber, 0, 0, 0, 0, 0, 0, 0, map);
        else if (upNumber + downNumber >= 2)
            DeleteElements(xPosition, yPosition, 0, 0, upNumber, downNumber, 0, 0, 0, 0, 0, map);
        else
            return false;

        return true;
    }

    private int GetRightElementsNumber(int xPosition, int yPosition, Cell[,] map)
    {
        if (xPosition + 1 >= HORIZONTAL_MAP_SIZE || map[yPosition, xPosition + 1] == null || map[yPosition, xPosition].Type != map[yPosition, xPosition + 1].Type)
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
        if (yPosition + 1 >= VERTICAL_MAP_SIZE || map[yPosition + 1, xPosition] == null || map[yPosition, xPosition].Type != map[yPosition + 1, xPosition].Type)
            return 0;

        return 1 + GetDownElementsNumber(xPosition, yPosition + 1, map);
    }

    /*
    private int GetUpRightElementsNumber(int xPosition, int yPosition, Cell[,] map)
    {
        if (xPosition + 1 >= HORIZONTAL_MAP_SIZE || yPosition - 1 < 0 || map[yPosition, xPosition].Type != map[yPosition - 1, xPosition + 1].Type)
            return 0;

        return 1 + GetUpRightElementsNumber(xPosition + 1, yPosition - 1, map);
    }
    private int GetUpLeftElementsNumber(int xPosition, int yPosition, Cell[,] map)
    {
        if (xPosition - 1 < 0 || yPosition - 1 < 0 || map[yPosition, xPosition].Type != map[yPosition - 1, xPosition - 1].Type)
            return 0;

        return 1 + GetUpLeftElementsNumber(xPosition - 1, yPosition - 1, map);
    }
    private int GetDownRightElementsNumber(int xPosition, int yPosition, Cell[,] map)
    {
        if (xPosition + 1 >= HORIZONTAL_MAP_SIZE || yPosition + 1 >= VERTICAL_MAP_SIZE || map[yPosition, xPosition].Type != map[yPosition + 1, xPosition + 1].Type)
            return 0;

        return 1 + GetDownRightElementsNumber(xPosition + 1, yPosition + 1, map);
    }
    private int GetDownLeftElementsNumber(int xPosition, int yPosition, Cell[,] map)
    {
        if (xPosition - 1 < 0 || yPosition + 1 >= VERTICAL_MAP_SIZE || map[yPosition, xPosition].Type != map[yPosition + 1, xPosition - 1].Type)
            return 0;

        return 1 + GetDownLeftElementsNumber(xPosition - 1, yPosition + 1, map);
    }
    */

    private void DeleteElements(int xPosition, int yPosition, int rightNumber, int leftNumber, int upNumber,
        int downNumber, int rightUpNumber, int leftUpNumber, int rightDownNumber, int leftDownNumber, int createdElement, Cell[,] map)
    {
        for (int i = 1; i <= rightNumber; i++)
        {
            Destroy(map[yPosition, xPosition + i].gameObject);
            map[yPosition, xPosition + i] = null;
        }
        for (int i = 1; i <= leftNumber; i++)
        {
            Destroy(map[yPosition, xPosition - i].gameObject);
            map[yPosition, xPosition - i] = null;
        }
        for (int i = 1; i <= upNumber; i++)
        {
            Destroy(map[yPosition - i, xPosition].gameObject);
            map[yPosition - i, xPosition] = null;
        }
        for (int i = 1; i <= downNumber; i++) 
        { 
            Destroy(map[yPosition + i, xPosition].gameObject);
            map[yPosition + i, xPosition] = null;
        }
        //for (int i = 1; i <= rightUpNumber; i++)
        //    Destroy(map[yPosition - i, xPosition + i].gameObject);
        //for (int i = 1; i <= leftUpNumber; i++)
        //    Destroy(map[yPosition - i, xPosition - i].gameObject);
        //for (int i = 1; i <= rightDownNumber; i++)
        //    Destroy(map[yPosition + i, xPosition + i].gameObject);
        //for (int i = 1; i <= leftDownNumber; i++)
        //    Destroy(map[yPosition + i, xPosition - i].gameObject);

        if (createdElement == 0)
        {
            Destroy(map[yPosition, xPosition].gameObject);
            map[yPosition, xPosition] = null;
        }
        else
        {
            map[yPosition, xPosition] = Instantiate(_cellPrefabs[createdElement - 1], map[yPosition, xPosition].transform.position, Quaternion.identity);
        }
    }

    private async Task MoveDownElements(Cell[,] map)
    {
        bool areElementsMoved = false, areElementsHandled = false;
        bool[,] needHandle = new bool[VERTICAL_MAP_SIZE, HORIZONTAL_MAP_SIZE];
        while (areElementsMoved == false || areElementsHandled == false)
        {
            areElementsMoved = true;
            for (int i = 0; i < HORIZONTAL_MAP_SIZE; i++)
            {
                for (int j = VERTICAL_MAP_SIZE - 1; j > 0; j--)
                {
                    if (map[j, i] != null)
                        continue;

                    areElementsMoved = false;

                    map[j, i] = map[j - 1, i];
                    map[j - 1, i] = null;
                    if (map[j, i] != null)
                    {
                        map[j, i].transform.position += Vector3.down * _interval;
                        needHandle[j, i] = true;
                    }
                }
                if (map[0, i] == null)
                {
                    map[0, i] = Instantiate(GetRandomElement(), new Vector2(-2.5f + _interval * i, 4f), Quaternion.identity);
                    needHandle[0, i] = true;
                }
            }
            await Task.Delay(500);
            areElementsHandled = true;
            for (int i = VERTICAL_MAP_SIZE - 1; i >= 0; i--)
            {
                for (int j = 0; j < HORIZONTAL_MAP_SIZE; j++)
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
    private Cell GetRandomElement()
    {
        int index = UnityEngine.Random.Range(0, _cellPrefabs.Count);
        if (index == _cellPrefabs.Count)
            index--;

        return _cellPrefabs[index];
    }
}
