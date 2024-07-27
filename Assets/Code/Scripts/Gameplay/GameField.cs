using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Zenject;
using Core.Gameplay.Input;
using Core.Infrastructure.Service;
using Core.Infrastructure.Factories;

namespace Core.Gameplay
{
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
        [Header("Audio Keys")]
        [SerializeField] private AudioPath _swipeAudio;
        [SerializeField] private AudioPath _destroyAudio;

        private ICellFabric _cellFabric;
        private IAbilityFactory _abilityFactory;
        private AudioService _audioService;
        private CellSwipeDetection _cellSwipeDetection;

        private Cell[,] _map;
    
        private bool _gameBlock = true;

        public int VerticalSize { get => _verticalMapSize; }
        public int HorizontalSize { get => _horizontalMapSize; }

        public event Action OnMove;
        public event Action<int> OnExplodeCellWithScore;

        [Serializable]
        private class CellConfig
        {
            public int X, Y;
            public CellType Type; 
        }

        [Inject]
        private void Construct(ICellFabric cellFabric, IAbilityFactory abilityFactory, AudioService audioService, CellSwipeDetection cellSwipeDetection)
        {
            _cellFabric = cellFabric;
            _abilityFactory = abilityFactory;
            _cellSwipeDetection = cellSwipeDetection;
            _cellSwipeDetection.OnTrySwipeCellWithGetDirection += Handle;
            _audioService = audioService;
        }

        private void OnDestroy()
        {
            _cellSwipeDetection.OnTrySwipeCellWithGetDirection -= Handle;
        }
        private async void Start()
        {
            _cellFabric.Init();
            _map = new Cell[_verticalMapSize, _horizontalMapSize];
            for(int i = 0; i < _cellConfigs.Length; i++)
            {
                _map[_cellConfigs[i].Y, _cellConfigs[i].X] = _cellFabric.GetCell(_cellConfigs[i].Type,
                    GetElementPosition(_cellConfigs[i].X, _cellConfigs[i].Y), Quaternion.identity, _cellContainer);
            }

            await MoveDownElements();
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

            OnMove?.Invoke();
            await SwapCells(firstXPosition, firstYPosition, secondXPosition, secondYPosition);
            Cell firstCell = _map[firstYPosition, firstXPosition];
            Cell secondCell = _map[secondYPosition, secondXPosition];
            bool isFirstElementMoved = false, isSecondElementMoved = false;
            if ((firstCell.IsSpecial && !secondCell.IsSpecial) || (!firstCell.IsSpecial && secondCell.IsSpecial))
            {
                Cell specialCell = firstCell.IsSpecial ? firstCell : secondCell;
                isFirstElementMoved = true;
                isSecondElementMoved = true;
                UseAbility(_abilityFactory.GetAbility(specialCell.Type), specialCell.transform.position);
            }
            else { 
                await UniTask.WhenAll(HandleMove(firstXPosition, firstYPosition), HandleMove(secondXPosition, secondYPosition))
                    .ContinueWith((result) =>
                    {
                        isFirstElementMoved = result.Item1;
                        isSecondElementMoved = result.Item2;
                    });
            }
                
            if (!isFirstElementMoved && !isSecondElementMoved)
                await SwapCells(firstXPosition, firstYPosition, secondXPosition, secondYPosition);
            else
                await MoveDownElements();

            _gameBlock = false;
        }
        public void UseAbility(IAbility ability, Vector3 cellPosition)
        {
            int firstXPosition = Mathf.RoundToInt((cellPosition.x - _startMapPoint.position.x) / _interval);
            int firstYPosition = Mathf.RoundToInt((_startMapPoint.position.y - cellPosition.y) / _interval);
            UseAbility(ability, firstXPosition, firstYPosition);
        }
        public async void UseAbility(IAbility ability, int xPosition, int yPosition)
        {
            OnMove?.Invoke();
            ability.Init(this);
            _gameBlock = true;

            await ability.Execute(xPosition, yPosition);
            await MoveDownElements();

            _gameBlock = false;
        }
        public async UniTask ExplodeCell(Cell cell)
        {
            int xPosition = Mathf.RoundToInt((cell.transform.position.x - _startMapPoint.position.x) / _interval);
            int yPosition = Mathf.RoundToInt((_startMapPoint.position.y - cell.transform.position.y) / _interval);
            await ExplodeCell(xPosition, yPosition);
        }
        public async UniTask ExplodeCell(int xPosition, int yPosition)
        {
            if (xPosition > _verticalMapSize - 1 || xPosition < 0 || yPosition > _horizontalMapSize - 1 || yPosition < 0 
                || _map[yPosition, xPosition].IsStatic || _map[yPosition, xPosition].IsMove || _map[yPosition, xPosition].IsExplode)
            {
                return;
            }
            _audioService.PlayOneShot(_destroyAudio);
            Cell cell = _map[yPosition, xPosition];
            await cell.Explode();
            if (cell.IsMove)
                Debug.LogError("Cell move when it's exploded!", cell);
            _map[yPosition, xPosition] = null;
            int score = cell.Score;
            _cellFabric.ReturnCell(cell);
            OnExplodeCellWithScore?.Invoke(score);
        }
        public Cell GetCell(int xPosition, int yPosition)
        {
            return _map[yPosition, xPosition];
        }
        public List<Cell> GetAllOfType(CellType type)
        {
            List<Cell> list = new List<Cell>();
            for(int i = 0; i < _horizontalMapSize; i++)
            {
                for(int j = 0; j < _verticalMapSize; j++)
                {
                    if (_map[j, i].Type == type)
                        list.Add(_map[j, i]);
                }
            }
            return list;
        }

        private async UniTask SwapCells(int firstXPosition, int firstYPosition, int secondXPosition, int secondYPosition)
        {
            Cell tmpCell = _map[firstYPosition, firstXPosition];
            Vector3 tmpPosition = tmpCell.transform.position;
            UniTask firstMoveTask = _map[firstYPosition, firstXPosition].MoveToWithTask(_map[secondYPosition, secondXPosition].transform.position, false);
            UniTask secondMoveTask = _map[secondYPosition, secondXPosition].MoveToWithTask(tmpPosition, false);
            _map[firstYPosition, firstXPosition] = _map[secondYPosition, secondXPosition];
            _map[secondYPosition, secondXPosition] = tmpCell;
            _audioService.PlayOneShot(_swipeAudio);

            await UniTask.WhenAll(firstMoveTask, secondMoveTask);
        }

        private async UniTask<bool> HandleMove(int xPosition, int yPosition)
        {
            if (_map[yPosition, xPosition] == null || _map[yPosition, xPosition].IsSpecial)
                return false;

            int rightNumber = GetRightElementsNumber(xPosition, yPosition);
            int leftNumber = GetLeftElementsNumber(xPosition, yPosition);
            int upNumber = GetUpElementsNumber(xPosition, yPosition);
            int downNumber = GetDownElementsNumber(xPosition, yPosition);

            if (upNumber + leftNumber >= 4)
                await DeleteElements(xPosition, yPosition, 0, leftNumber, upNumber, 0, CellType.Bomb);
            else if (upNumber + rightNumber >= 4)
                await DeleteElements(xPosition, yPosition, rightNumber, 0, upNumber, 0, CellType.Bomb);
            else if (downNumber + leftNumber >= 4)
                await DeleteElements(xPosition, yPosition, 0, leftNumber, 0, downNumber, CellType.Bomb);
            else if (downNumber + rightNumber >= 4)
                await DeleteElements(xPosition, yPosition, rightNumber, 0, 0, downNumber, CellType.Bomb);
            else if (rightNumber + leftNumber >= 2)
                await DeleteElements(xPosition, yPosition, rightNumber, leftNumber, 0, 0, 0);
            else if (upNumber + downNumber >= 2)
                await DeleteElements(xPosition, yPosition, 0, 0, upNumber, downNumber, 0);
            else
                return false;

            return true;
        }

        private int GetRightElementsNumber(int xPosition, int yPosition)
        {
            if (xPosition + 1 >= _horizontalMapSize || _map[yPosition, xPosition + 1] == null ||
                _map[yPosition, xPosition + 1].IsMove ||
                _map[yPosition, xPosition + 1].IsExplode || _map[yPosition, xPosition].Type != _map[yPosition, xPosition + 1].Type)
                return 0;

            return 1 + GetRightElementsNumber(xPosition + 1, yPosition);
        }
        private int GetLeftElementsNumber(int xPosition, int yPosition)
        {
            if (xPosition - 1 < 0 || _map[yPosition, xPosition - 1] == null || _map[yPosition, xPosition - 1].IsMove ||
                _map[yPosition, xPosition - 1].IsExplode ||
                _map[yPosition, xPosition].Type != _map[yPosition, xPosition - 1].Type)
                return 0;

            return 1 + GetLeftElementsNumber(xPosition - 1, yPosition);
        }
        private int GetUpElementsNumber(int xPosition, int yPosition)
        {
            if (yPosition - 1 < 0 || _map[yPosition - 1, xPosition] == null || _map[yPosition - 1, xPosition].IsMove ||
                _map[yPosition - 1, xPosition].IsExplode ||
                _map[yPosition, xPosition].Type != _map[yPosition - 1, xPosition].Type)
                return 0;

            return 1 + GetUpElementsNumber(xPosition, yPosition - 1);
        }
        private int GetDownElementsNumber(int xPosition, int yPosition)
        {
            if (yPosition + 1 >= _verticalMapSize || _map[yPosition + 1, xPosition] == null || _map[yPosition + 1, xPosition].IsMove ||
                _map[yPosition + 1, xPosition].IsExplode ||
                _map[yPosition, xPosition].Type != _map[yPosition + 1, xPosition].Type)
                return 0;

            return 1 + GetDownElementsNumber(xPosition, yPosition + 1);
        }

        private async UniTask DeleteElements(int xPosition, int yPosition, int rightNumber, int leftNumber, int upNumber,
            int downNumber, CellType createdElement)
        {
            UniTask[] tasks = new UniTask[rightNumber + leftNumber + upNumber + downNumber + 1];
            int generalIndex = 0;
            for (int i = 1; i <= rightNumber; i++)
            {
                tasks[generalIndex] = ExplodeCell(xPosition + i, yPosition);
                generalIndex++;
            }
            for (int i = 1; i <= leftNumber; i++)
            {
                tasks[generalIndex] = ExplodeCell(xPosition - i, yPosition);
                generalIndex++;
            }
            for (int i = 1; i <= upNumber; i++)
            {
                tasks[generalIndex] = ExplodeCell(xPosition, yPosition - i);
                generalIndex++;
            }
            for (int i = 1; i <= downNumber; i++) 
            {
                tasks[generalIndex] = ExplodeCell(xPosition, yPosition + i);
                generalIndex++;
            }

            tasks[generalIndex] = ExplodeCell(xPosition, yPosition);
            await UniTask.WhenAll(tasks);
            if (createdElement != 0)
            {
                _map[yPosition, xPosition] = 
                    _cellFabric.GetCell(createdElement, GetElementPosition(xPosition, yPosition), Quaternion.identity, _cellContainer);
            }
        }

        private async UniTask MoveDownElements()
        {
            int x = 0, y = 0;
            bool areElementsMoved = false;
            do
            {
                areElementsMoved = true;
                for (int i = 0; i < _horizontalMapSize; i++)
                {
                    int lowerElementIndex = 0;
                    for (int j = _verticalMapSize - 1; j >= 0; j--)
                    {
                        if (_map[j, i] == null)
                        {
                            areElementsMoved = false;
                            if (lowerElementIndex < j)
                                lowerElementIndex = j;
                        }
                        else
                        {
                            if (_map[j, i].IsStatic)
                                continue;

                            if (_map[j, i].IsMove || _map[j, i].IsExplode)
                                areElementsMoved = false;
                            if (_map[j, i].IsExplode)
                            {
                                lowerElementIndex = 0;
                                continue;
                            }

                            if (lowerElementIndex <= j)
                                continue;


                            areElementsMoved = false;
                            _map[lowerElementIndex, i] = _map[j, i];
                            _map[j, i] = null;
                            _map[lowerElementIndex, i].MoveTo(GetElementPosition(i, lowerElementIndex), true, DoCallback);
                            lowerElementIndex--;
                        }
                    }
                    int upperElementIndex, spawnQueue = 0;
                    for (upperElementIndex = 0; upperElementIndex < _verticalMapSize; upperElementIndex++)
                    {
                        if (_map[upperElementIndex, i] == null || !_map[upperElementIndex, i].IsStatic)
                            break;
                    }
                    for (int j = _verticalMapSize - 1; j >= 0; j--)
                    {
                        if (_map[j, i] != null)
                            continue;

                        spawnQueue++;
                        Vector2 pos = GetElementPosition(i, upperElementIndex - spawnQueue);
                        _map[j, i] = _cellFabric.GetCell(GetRandomElementType(),
                            new Vector3(pos.x, pos.y, 0), Quaternion.identity, _cellContainer);
                        _map[j, i].MoveTo(GetElementPosition(i, j), true, DoCallback);
                        areElementsMoved = false;
                    }
                }
                await UniTask.Yield();
            } while (!areElementsMoved);

            async void DoCallback(Cell cell)
            {
                x = Mathf.RoundToInt((cell.transform.position.x - _startMapPoint.position.x) / _interval);
                y = Mathf.RoundToInt((_startMapPoint.position.y - cell.transform.position.y) / _interval);
                bool handled = await HandleMove(x, y);
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
}
