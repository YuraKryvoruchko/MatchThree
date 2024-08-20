using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Zenject;
using Core.Gameplay.Input;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Factories;
using Core.Infrastructure.Service.Pause;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using com.cyborgAssets.inspectorButtonPro;
#endif

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
        [SerializeField] private ClipEvent _swipeAudio;
        [SerializeField] private ClipEvent _destroyAudio;

        private ICellFabric _cellFabric;
        private IAbilityFactory _abilityFactory;
        private IPauseProvider _pauseProvider;
        private IAudioService _audioService;
        private CellSwipeDetection _cellSwipeDetection;

        private Cell[,] _map;

        private List<IAbility> _usedAbilities;

        private bool _gameBlock = true;
        private bool _cellsMovesDown = false;

        public int VerticalSize { get => _verticalMapSize; }
        public int HorizontalSize { get => _horizontalMapSize; }

        public event Action OnMove;
        public event Action<int> OnExplodeCellWithScore;

        [Serializable]
        private class CellConfig
        {
            public Vector2Int Position;
            public CellType Type; 
        }

        [Inject]
        private void Construct(ICellFabric cellFabric, IAbilityFactory abilityFactory, IPauseProvider pauseProvider,
            IAudioService audioService, CellSwipeDetection cellSwipeDetection)
        {
            _usedAbilities = new List<IAbility>();
            _cellFabric = cellFabric;
            _abilityFactory = abilityFactory;
            _pauseProvider = pauseProvider;
            _pauseProvider.OnPause += HandlePause;
            _cellSwipeDetection = cellSwipeDetection;
            _cellSwipeDetection.OnTrySwipeCellWithGetDirection += Handle;
            _audioService = audioService;
        }

#if UNITY_EDITOR
        [ProPlayButton]
        private void ReplaceCellTest(Vector2Int cellPosition, CellType type)
        {
            ReplaceCell(type, cellPosition);
        }
#endif

        private void OnDestroy()
        {
            _cellSwipeDetection.OnTrySwipeCellWithGetDirection -= Handle;
            _pauseProvider.OnPause -= HandlePause;
        }
        private async void Start()
        {
            _cellFabric.Init();
            _map = new Cell[_verticalMapSize, _horizontalMapSize];
            for(int i = 0; i < _cellConfigs.Length; i++)
            {
                Vector2Int cellPosition = _cellConfigs[i].Position;
                _map[cellPosition.y, cellPosition.x] = _cellFabric.GetCell(_cellConfigs[i].Type,
                    CellPositionToWorld(cellPosition), Quaternion.identity, _cellContainer);
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

            Vector2Int firstPosition = WorldPositionToCell(cellPosition);
            Vector2Int secondPosition = new Vector2Int(firstPosition.x + Mathf.RoundToInt(swipeDirection.x), firstPosition.y - Mathf.RoundToInt(swipeDirection.y));

            if (!IsPositionInBoard(firstPosition) || !IsPositionInBoard(secondPosition) ||
                _map[firstPosition.y, firstPosition.x].IsStatic || _map[secondPosition.y, secondPosition.x].IsStatic)
            {
                _gameBlock = false;
                return;
            }

            OnMove?.Invoke();
            await SwapCells(firstPosition, secondPosition);

            Cell firstCell = _map[firstPosition.y, firstPosition.x];
            Cell secondCell = _map[secondPosition.y, secondPosition.x];
            bool isFirstElementMoved = false, isSecondElementMoved = false;
            if (firstCell.IsSpecial && !secondCell.IsSpecial)
            {
                isFirstElementMoved = true;
                isSecondElementMoved = true;
                UseAbility(_abilityFactory.GetAbility(firstCell.Type), secondPosition, firstPosition);
            }
            else if (!firstCell.IsSpecial && secondCell.IsSpecial)
            {
                isFirstElementMoved = true;
                isSecondElementMoved = true;
                UseAbility(_abilityFactory.GetAbility(secondCell.Type), firstPosition, secondPosition);
            }
            else if (firstCell.IsSpecial && secondCell.IsSpecial)
            {
                isFirstElementMoved = true;
                isSecondElementMoved = true;
                UseAbility(_abilityFactory.GetAdvancedAbility(firstCell.Type, secondCell.Type), firstPosition, secondPosition);
            }
            else { 
                await UniTask.WhenAll(HandleMove(firstPosition), HandleMove(secondPosition))
                    .ContinueWith((result) =>
                    {
                        isFirstElementMoved = result.Item1;
                        isSecondElementMoved = result.Item2;
                    });
            }
                
            if (!isFirstElementMoved && !isSecondElementMoved)
                await SwapCells(firstPosition, secondPosition);
            else
                await MoveDownElements();

            _gameBlock = false;
        }

        public async void UseAbility(IAbility ability, Vector2Int swipedCellPosition, Vector2Int abilityPosition)
        {
            OnMove?.Invoke();
            ability.Init(this);
            _gameBlock = true;

            _usedAbilities.Add(ability);

            await ability.Execute(swipedCellPosition, abilityPosition);
            _usedAbilities.Remove(ability);
            await MoveDownElements();

            _gameBlock = false;
        }

        public async UniTask ExplodeCell(Vector2Int cellPosition)
        {
            if (cellPosition.x > _verticalMapSize - 1 || cellPosition.x < 0 || cellPosition.y > _horizontalMapSize - 1 || cellPosition.y < 0 
                || _map[cellPosition.y, cellPosition.x] == null
                || _map[cellPosition.y, cellPosition.x].IsStatic 
                || _map[cellPosition.y, cellPosition.x].IsMove 
                || _map[cellPosition.y, cellPosition.x].IsExplode)
            {
                return;
            }

            _audioService.PlayOneShot(_destroyAudio);

            Cell cell = _map[cellPosition.y, cellPosition.x];
            await cell.Explode();
            if (cell.IsMove)
            {
                Debug.LogWarning("Cell move when it's exploded!", cell);
                cell.StopMove();
            }

            _map[cellPosition.y, cellPosition.x] = null;
            int score = cell.Score;
            _cellFabric.ReturnCell(cell);
            OnExplodeCellWithScore?.Invoke(score);

            MoveDownElements().Forget();
        }
        public void ReplaceCell(CellType newType, Vector2Int cellPosition)
        {
            _cellFabric.ReturnCell(_map[cellPosition.y, cellPosition.x]);
            Vector2 worldPosition = CellPositionToWorld(cellPosition);
            _map[cellPosition.y, cellPosition.x] = _cellFabric.GetCell(newType, worldPosition, Quaternion.identity, _cellContainer);
        }

        public Cell GetCell(Vector2Int cellPosition)
        {
            return _map[cellPosition.y, cellPosition.x];
        }
        public List<Cell> GetAllOfType(CellType type)
        {
            List<Cell> list = new List<Cell>();
            for(int i = 0; i < _horizontalMapSize; i++)
            {
                for(int j = 0; j < _verticalMapSize; j++)
                {
                    if(_map[j, i] == null)
                        continue;

                    if (_map[j, i].Type == type)
                        list.Add(_map[j, i]);
                }
            }
            return list;
        }
        public List<Cell> GetBy—ondition(Func<Cell, bool> condition)
        {
            List<Cell> list = new List<Cell>();
            for (int i = 0; i < _horizontalMapSize; i++)
            {
                for (int j = 0; j < _verticalMapSize; j++)
                {
                    if (condition.Invoke(_map[j, i]))
                        list.Add(_map[j, i]);
                }
            }
            return list;
        }

        public Vector2Int WorldPositionToCell(Vector3 position)
        {
            int x = Mathf.RoundToInt((position.x - _startMapPoint.position.x) / _interval);
            int y = Mathf.RoundToInt((_startMapPoint.position.y - position.y) / _interval);
            return new Vector2Int(x, y);
        }
        public Vector2 CellPositionToWorld(Vector2Int cellPosition)
        {
            return new Vector2(_startMapPoint.position.x + _interval * cellPosition.x, 
                _startMapPoint.position.y - _interval * cellPosition.y);
        }

        private async UniTask SwapCells(Vector2Int firstPosition, Vector2Int secondPosition)
        {
            Cell firstCell = _map[firstPosition.y, firstPosition.x], secondCell = _map[secondPosition.y, secondPosition.x];
            Vector3 tmpPosition = firstCell.transform.position;
            UniTask firstMoveTask = firstCell.MoveToWithTask(secondCell.transform.position, false);
            UniTask secondMoveTask = secondCell.MoveToWithTask(tmpPosition, false);
            _map[firstPosition.y, firstPosition.x] = secondCell;
            _map[secondPosition.y, secondPosition.x] = firstCell;
            _audioService.PlayOneShot(_swipeAudio);

            await UniTask.WhenAll(firstMoveTask, secondMoveTask);
        }

        private async UniTask<bool> HandleMove(Vector2Int cellPosition)
        {
            if (_map[cellPosition.y, cellPosition.x] == null || _map[cellPosition.y, cellPosition.x].IsSpecial)
                return false;

            int rightNumber = GetElementsNumberOnDirection(cellPosition, Vector2Int.right);
            int leftNumber = GetElementsNumberOnDirection(cellPosition, Vector2Int.left);
            int upNumber = GetElementsNumberOnDirection(cellPosition, Vector2Int.down);
            int downNumber = GetElementsNumberOnDirection(cellPosition, Vector2Int.up);

            int rightUpNumber = GetElementsNumberOnDirection(cellPosition, Vector2Int.right + Vector2Int.down);
            int rightDownNumber = GetElementsNumberOnDirection(cellPosition, Vector2Int.right + Vector2Int.up);
            int leftUpNumber = GetElementsNumberOnDirection(cellPosition, Vector2Int.left + Vector2Int.down);
            int leftDownNumber = GetElementsNumberOnDirection(cellPosition, Vector2Int.left + Vector2Int.up);

            if (upNumber + downNumber >= 4)
            {
                await UniTask.WhenAll(
                    DeleteElementsOnDirection(cellPosition, Vector2Int.down, upNumber),
                    DeleteElementsOnDirection(cellPosition, Vector2Int.up, downNumber),
                    ExplodeCell(cellPosition)
                );
                _map[cellPosition.y, cellPosition.x] =
                    _cellFabric.GetCell(CellType.Supper, CellPositionToWorld(cellPosition), Quaternion.identity, _cellContainer);
            }
            else if (leftNumber + rightNumber >= 4)
            {
                await UniTask.WhenAll(
                    DeleteElementsOnDirection(cellPosition, Vector2Int.left, leftNumber),
                    DeleteElementsOnDirection(cellPosition, Vector2Int.right, rightNumber),
                    ExplodeCell(cellPosition)
                );
                _map[cellPosition.y, cellPosition.x] =
                    _cellFabric.GetCell(CellType.Supper, CellPositionToWorld(cellPosition), Quaternion.identity, _cellContainer);
            }
            else if (upNumber + leftNumber >= 4)
            {
                await UniTask.WhenAll(
                    DeleteElementsOnDirection(cellPosition, Vector2Int.down, upNumber),
                    DeleteElementsOnDirection(cellPosition, Vector2Int.left, leftNumber),
                    ExplodeCell(cellPosition)
                );
                _map[cellPosition.y, cellPosition.x] =
                    _cellFabric.GetCell(CellType.Bomb, CellPositionToWorld(cellPosition), Quaternion.identity, _cellContainer);
            }
            else if (upNumber + rightNumber >= 4)
            {
                await UniTask.WhenAll(
                    DeleteElementsOnDirection(cellPosition, Vector2Int.down, upNumber),
                    DeleteElementsOnDirection(cellPosition, Vector2Int.right, rightNumber),
                    ExplodeCell(cellPosition)
                );
                _map[cellPosition.y, cellPosition.x] =
                    _cellFabric.GetCell(CellType.Bomb, CellPositionToWorld(cellPosition), Quaternion.identity, _cellContainer);
            }
            else if (downNumber + leftNumber >= 4)
            {
                await UniTask.WhenAll(
                    DeleteElementsOnDirection(cellPosition, Vector2Int.up, downNumber),
                    DeleteElementsOnDirection(cellPosition, Vector2Int.left, leftNumber),
                    ExplodeCell(cellPosition)
                );
                _map[cellPosition.y, cellPosition.x] =
                    _cellFabric.GetCell(CellType.Bomb, CellPositionToWorld(cellPosition), Quaternion.identity, _cellContainer);
            }
            else if (downNumber + rightNumber >= 4)
            {
                await UniTask.WhenAll(
                    DeleteElementsOnDirection(cellPosition, Vector2Int.up, downNumber), 
                    DeleteElementsOnDirection(cellPosition, Vector2Int.right, rightNumber),
                    ExplodeCell(cellPosition)
                );
                _map[cellPosition.y, cellPosition.x] =
                    _cellFabric.GetCell(CellType.Bomb, CellPositionToWorld(cellPosition), Quaternion.identity, _cellContainer);
            }
            else if (rightNumber + upNumber >= 2 && rightUpNumber >= 1)
            {
                await UniTask.WhenAll(
                    DeleteElementsOnDirection(cellPosition, Vector2Int.down, 1),
                    DeleteElementsOnDirection(cellPosition, Vector2Int.right, 1),
                    DeleteElementsOnDirection(cellPosition, Vector2Int.right + Vector2Int.down, 1),
                    ExplodeCell(cellPosition)
                );
                _map[cellPosition.y, cellPosition.x] =
                    _cellFabric.GetCell(CellType.LightningBolt, CellPositionToWorld(cellPosition), Quaternion.identity, _cellContainer);
            }
            else if (rightNumber + downNumber >= 2 && rightDownNumber >= 1)
            {
                await UniTask.WhenAll(
                    DeleteElementsOnDirection(cellPosition, Vector2Int.up, 1),
                    DeleteElementsOnDirection(cellPosition, Vector2Int.right, 1),
                    DeleteElementsOnDirection(cellPosition, Vector2Int.right + Vector2Int.up, 1),
                    ExplodeCell(cellPosition)
                );
                _map[cellPosition.y, cellPosition.x] =
                    _cellFabric.GetCell(CellType.LightningBolt, CellPositionToWorld(cellPosition), Quaternion.identity, _cellContainer);
            }
            else if (leftNumber + upNumber >= 2 && leftUpNumber >= 1)
            {
                await UniTask.WhenAll(
                    DeleteElementsOnDirection(cellPosition, Vector2Int.down, 1),
                    DeleteElementsOnDirection(cellPosition, Vector2Int.left, 1),
                    DeleteElementsOnDirection(cellPosition, Vector2Int.left + Vector2Int.down, 1),
                    ExplodeCell(cellPosition)
                );
                _map[cellPosition.y, cellPosition.x] =
                    _cellFabric.GetCell(CellType.LightningBolt, CellPositionToWorld(cellPosition), Quaternion.identity, _cellContainer);
            }
            else if (leftNumber + downNumber >= 2 && leftDownNumber >= 1)
            {
                await UniTask.WhenAll(
                    DeleteElementsOnDirection(cellPosition, Vector2Int.up, 1),
                    DeleteElementsOnDirection(cellPosition, Vector2Int.left, 1),
                    DeleteElementsOnDirection(cellPosition, Vector2Int.left + Vector2Int.up, 1),
                    ExplodeCell(cellPosition)
                );
                _map[cellPosition.y, cellPosition.x] =
                    _cellFabric.GetCell(CellType.LightningBolt, CellPositionToWorld(cellPosition), Quaternion.identity, _cellContainer);
            }
            else if (rightNumber + leftNumber >= 2)
            {
                await UniTask.WhenAll(
                    DeleteElementsOnDirection(cellPosition, Vector2Int.right, rightNumber),
                    DeleteElementsOnDirection(cellPosition, Vector2Int.left, leftNumber),
                    ExplodeCell(cellPosition)
                );
            }
            else if (upNumber + downNumber >= 2)
            {
                await UniTask.WhenAll(
                    DeleteElementsOnDirection(cellPosition, Vector2Int.down, upNumber),
                    DeleteElementsOnDirection(cellPosition, Vector2Int.up, downNumber),
                    ExplodeCell(cellPosition)
                );
            }
            else
            {
                return false;
            }

            return true;
        }
        private void HandlePause(bool isPause)
        {
            for(int i = 0; i < _horizontalMapSize; i++)
            {
                for(int j = _verticalMapSize - 1; j >= 0; j--)
                    _map[j, i].SetPause(isPause);
            }
            foreach (IAbility ability in _usedAbilities)
                ability.SetPause(isPause);
        }

        private int GetElementsNumberOnDirection(Vector2Int position, Vector2Int direction)
        {
            Vector2Int newPosition = position + direction;
            if (newPosition.x < 0 || newPosition.x >= _horizontalMapSize || newPosition.y < 0 || newPosition.y >= _verticalMapSize || 
                _map[newPosition.y, newPosition.x] == null || _map[newPosition.y, newPosition.x].IsMove || _map[newPosition.y, newPosition.x].IsExplode ||
                _map[position.y, position.x].Type != _map[newPosition.y, newPosition.x].Type)
            {
                return 0;
            }

            return 1 + GetElementsNumberOnDirection(newPosition, direction);
        }

        private async UniTask DeleteElementsOnDirection(Vector2Int cellPosition, Vector2Int direction, int lenght)
        {
            UniTask[] tasks = new UniTask[lenght];

            for (int i = 1; i <= lenght; i++)
            {
                tasks[i - 1] = ExplodeCell(new Vector2Int(cellPosition.x + direction.x * i, cellPosition.y + direction.y * i));
            }

            await UniTask.WhenAll(tasks);
        }

        private async UniTask MoveDownElements()
        {
            if (_cellsMovesDown)
                return;

            _cellsMovesDown = true;
            bool areElementsMoved = false;
            do
            {
                areElementsMoved = true;
                for (int i = 0; i < _horizontalMapSize; i++)
                {
                    int lowerElementIndex = 0, spawnQueue = 0;
                    for (int j = _verticalMapSize - 1; j >= 0; j--)
                    {
                        if (_map[j, i] == null)
                        {
                            spawnQueue++;
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
                                spawnQueue = 0;
                                continue;
                            }

                            if (lowerElementIndex <= j)
                                continue;

                            areElementsMoved = false;
                            _map[lowerElementIndex, i] = _map[j, i];
                            _map[j, i] = null;
                            _map[lowerElementIndex, i].MoveTo(CellPositionToWorld(new Vector2Int(i, lowerElementIndex)), true, DoCallback);
                            lowerElementIndex--;
                        }
                    }
                    int upperElementIndex;
                    for (upperElementIndex = 0; upperElementIndex < _verticalMapSize; upperElementIndex++)
                    {
                        if (_map[upperElementIndex, i] == null || !_map[upperElementIndex, i].IsStatic)
                            break;
                    }
                    for (int j = upperElementIndex; j < _verticalMapSize && spawnQueue > 0; j++)
                    {
                        if (_map[j, i] != null)
                            continue;

                        Vector2 pos = CellPositionToWorld(new Vector2Int(i, upperElementIndex - spawnQueue));
                        _map[j, i] = _cellFabric.GetCell(GetRandomElementType(),
                            new Vector3(pos.x, pos.y, 0), Quaternion.identity, _cellContainer);
                        _map[j, i].MoveTo(CellPositionToWorld(new Vector2Int(i, j)), true, DoCallback);
                        areElementsMoved = false;
                        spawnQueue--;
                    }
                }
                await UniTask.Yield();
            } while (!areElementsMoved);

            _cellsMovesDown = false;

            async void DoCallback(Cell cell)
            {
                bool handled = await HandleMove(WorldPositionToCell(cell.transform.position));
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
        private bool IsPositionInBoard(Vector2Int position)
        {
            return position.x < _horizontalMapSize && position.x >= 0 && position.y < _verticalMapSize && position.y >= 0;
        }
    }
}
