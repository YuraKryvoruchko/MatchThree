using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Zenject;
using Core.Gameplay.Input;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Factories;
using Core.Infrastructure.Service.Pause;
using static UnityEditor.PlayerSettings;


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

        private int _currentFindingCombinationIndex;
        private int[,] _findingCombinationIndexMap;

        private bool _needHandleCells;
        private bool[,] _cellHandlingMap;

        private bool _isBoardFillUp = false;
        private bool _isSwapHandling = true;

        public int VerticalSize { get => _verticalMapSize; }
        public int HorizontalSize { get => _horizontalMapSize; }
        public bool IsBoardFillUp { get => _isBoardFillUp; }
        public bool IsBoardPlay { get => _usedAbilities.Count > 0 || _isBoardFillUp; }

        public event Action OnMove;
        public event Action<int> OnExplodeCellWithScore;
        public event Action<CellExplosionResult> OnExplodeCellWithResult;
        public event Action<bool> OnPause;

        [Serializable]
        private class CellConfig
        {
            public Vector2Int Position = Vector2Int.zero;
            public CellType Type = 0;
        }

        private struct SearchResult
        {
            public int ScoreNumber;
            public Vector2Int CellPosition;

            public SearchResult(int scoreNumber, Vector2Int cellPosition)
            {
                ScoreNumber = scoreNumber;
                CellPosition = cellPosition;
            }
        }
        public struct CellExplosionResult
        {
            public int Score;
            public CellType Type;
        }

        [Inject]
        private void Construct(ICellFabric cellFabric, IAbilityFactory abilityFactory, IPauseProvider pauseProvider,
            IAudioService audioService, CellSwipeDetection cellSwipeDetection)
        {
            _cellFabric = cellFabric;
            _abilityFactory = abilityFactory;
            _pauseProvider = pauseProvider;
            _cellSwipeDetection = cellSwipeDetection;
            _audioService = audioService;
            _usedAbilities = new List<IAbility>();
        }

#if UNITY_EDITOR
        [ProPlayButton]
        private void ReplaceCellTest(Vector2Int cellPosition, CellType type)
        {
            ReplaceCell(type, cellPosition);
        }
#endif
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

        public void Init()
        {
            _pauseProvider.OnPause += HandlePause;
            _cellSwipeDetection.OnTrySwipeCellWithGetDirection += HandleSwipeCellWithDirection;

            _cellFabric.Init();
            _map = new Cell[_verticalMapSize, _horizontalMapSize];
            _findingCombinationIndexMap = new int[_verticalMapSize, _horizontalMapSize];
            _cellHandlingMap = new bool[_verticalMapSize, _horizontalMapSize];
            for (int i = 0; i < _cellConfigs.Length; i++)
            {
                Vector2Int cellPosition = _cellConfigs[i].Position;
                _map[cellPosition.y, cellPosition.x] = _cellFabric.GetCell(_cellConfigs[i].Type,
                    CellPositionToWorld(cellPosition), Quaternion.identity, _cellContainer);
            }

            FillBoardAsync().Forget();
        }
        public void Deinit()
        {
            _cellSwipeDetection.OnTrySwipeCellWithGetDirection -= HandleSwipeCellWithDirection;
            _pauseProvider.OnPause -= HandlePause;
        }

        public async UniTaskVoid Handle(Vector2 cellPosition, Vector2 swipeDirection)
        {
            if (!_isSwapHandling)
                return;

            swipeDirection = swipeDirection.normalized;

            Vector2Int firstPosition = WorldPositionToCell(cellPosition);
            Vector2Int secondPosition = new Vector2Int(firstPosition.x + Mathf.RoundToInt(swipeDirection.x), firstPosition.y - Mathf.RoundToInt(swipeDirection.y));

            if (!IsPositionInBoard(firstPosition) || !IsPositionInBoard(secondPosition) 
                || !CanHandleCellForSwipe(GetCell(firstPosition)) || !CanHandleCellForSwipe(GetCell(secondPosition)))
            {
                return;
            }

            OnMove?.Invoke();
            await SwapCellsAsync(firstPosition, secondPosition);

            Cell firstCell = _map[firstPosition.y, firstPosition.x];
            Cell secondCell = _map[secondPosition.y, secondPosition.x];
            bool isFirstElementMoved = true, isSecondElementMoved = true;
            if (firstCell.IsSpecial && !secondCell.IsSpecial)
            {
                UseAbility(firstCell.Type, secondPosition, firstPosition);
            }
            else if (!firstCell.IsSpecial && secondCell.IsSpecial)
            {
                UseAbility(secondCell.Type, firstPosition, secondPosition);
            }
            else if (firstCell.IsSpecial && secondCell.IsSpecial)
            {
                UseAbility(secondCell.Type, firstPosition, secondPosition);
            }
            else {
                isFirstElementMoved = HandleMove(firstPosition);
                isSecondElementMoved = HandleMove(secondPosition);
            }
                
            if (!isFirstElementMoved && !isSecondElementMoved)
                await SwapCellsAsync(firstPosition, secondPosition);
            else
                TryFillBoard();
        }

        public void UseAbility(CellType abilityType, Vector2Int swipedCellPosition, Vector2Int abilityPosition)
        {
            Cell swipedCell = GetCell(swipedCellPosition);
            IAbility ability = swipedCell.IsSpecial ? _abilityFactory.GetAdvancedAbility(abilityType, swipedCell.Type)
                : _abilityFactory.GetAbility(abilityType);

            ability.Init(this);
            ability.Execute(swipedCellPosition, abilityPosition, HandleAbilityCallback).Forget();
            _usedAbilities.Add(ability);
            OnMove?.Invoke();
        }

        public async UniTask ExplodeCellAsync(Vector2Int cellPosition)
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
            _cellHandlingMap[cellPosition.y, cellPosition.x] = false;
            Cell cell = _map[cellPosition.y, cellPosition.x];
            await cell.Explode();
            if (cell.IsMove)
            {
                Debug.LogWarning("Cell move when it's exploded!", cell);
                cell.StopMove();
            }

            _map[cellPosition.y, cellPosition.x] = null;
            int score = cell.Score;
            CellType type = cell.Type;
            _cellFabric.ReturnCell(cell);

            OnExplodeCellWithScore?.Invoke(score);
            OnExplodeCellWithResult?.Invoke(new CellExplosionResult() { Score = score, Type = type });

            TryFillBoard();
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

        public void SetSwipeHandlingStatus(bool isSwipeHandling)
        {
            _isSwapHandling = isSwipeHandling;
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

        private void HandleSwipeCellWithDirection(Vector2 cellPosition, Vector2 swipeDirection)
        {
            Handle(cellPosition, swipeDirection).Forget();
        }
        private async UniTask SwapCellsAsync(Vector2Int firstPosition, Vector2Int secondPosition)
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

        private bool HandleMove(Vector2Int cellPosition)
        {
            const int SEARCH_DEPTH = 10;

            _currentFindingCombinationIndex++;
            SearchResult bestResult = FindMaxScoreCombination(cellPosition, SEARCH_DEPTH);
            if (bestResult.ScoreNumber == 0)
                return false;

            return ExplodeCombinationForCell(bestResult.CellPosition);
        }
        private SearchResult FindMaxScoreCombination(Vector2Int cellPosition, int depth)
        {
            if (depth <= 0 || _findingCombinationIndexMap[cellPosition.y, cellPosition.x] == _currentFindingCombinationIndex)
                return new SearchResult(0, cellPosition);

            _findingCombinationIndexMap[cellPosition.y, cellPosition.x] = _currentFindingCombinationIndex;

            int scoreOnThisPoint = GetScore(cellPosition);
            SearchResult[] results = new SearchResult[4];
            Cell currentCell = GetCell(cellPosition);
            if (CanHandleCellForGetScore(cellPosition + Vector2Int.left, currentCell.Type))
            {
                results[0] = FindMaxScoreCombination(cellPosition + Vector2Int.left, depth - 1);
            }
            else if (CanHandleCellForGetScore(cellPosition + Vector2Int.right, currentCell.Type))
            {
                results[1] = FindMaxScoreCombination(cellPosition + Vector2Int.right, depth - 1);
            }
            else if (CanHandleCellForGetScore(cellPosition + Vector2Int.up, currentCell.Type))
            {
                results[2] = FindMaxScoreCombination(cellPosition + Vector2Int.up, depth - 1);
            }
            else if (CanHandleCellForGetScore(cellPosition + Vector2Int.down, currentCell.Type))
            {
                results[3] = FindMaxScoreCombination(cellPosition + Vector2Int.down, depth - 1);
            }

            SearchResult bestResult = new SearchResult(scoreOnThisPoint, cellPosition);
            for (int i = 0; i < results.Length; i++)
            {
                if (results[i].ScoreNumber > bestResult.ScoreNumber)
                    bestResult = results[i];
            }

            return bestResult;
        }
        private int GetScore(Vector2Int cellPosition)
        {
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
                return 6;
            }
            else if (leftNumber + rightNumber >= 4)
            {
                return 6;
            }
            else if (upNumber >= 2 && leftNumber >= 2)
            {
                return 5;
            }
            else if (upNumber >= 2 && rightNumber >= 2)
            {
                return 5;
            }
            else if (downNumber >= 2 && leftNumber >= 2)
            {
                return 5;
            }
            else if (downNumber >= 2 && rightNumber >= 2)
            {
                return 5;
            }
            else if (rightNumber >= 1 && upNumber >= 1 && rightUpNumber >= 1)
            {
                return 4;
            }
            else if (rightNumber >= 1 && downNumber >= 1 && rightDownNumber >= 1)
            {
                return 4;
            }
            else if (leftNumber >= 1 && upNumber >= 1 && leftUpNumber >= 1)
            {
                return 4;
            }
            else if (leftNumber >= 1 && downNumber >= 1 && leftDownNumber >= 1)
            {
                return 4;
            }
            else if (rightNumber + leftNumber >= 2)
            {
                return rightNumber + leftNumber;
            }
            else if (upNumber + downNumber >= 2)
            {
                return upNumber + downNumber;
            }
            else
            {
                return 0;
            }
        }

        private bool ExplodeCombinationForCell(Vector2Int cellPosition)
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
                DeleteElementsOnDirection(cellPosition, Vector2Int.down, upNumber);
                DeleteElementsOnDirection(cellPosition, Vector2Int.up, downNumber);
                DeleteElementAndReplace(cellPosition, CellType.Supper).Forget();
            }
            else if (leftNumber + rightNumber >= 4)
            {
                DeleteElementsOnDirection(cellPosition, Vector2Int.left, leftNumber);
                DeleteElementsOnDirection(cellPosition, Vector2Int.right, rightNumber);
                DeleteElementAndReplace(cellPosition, CellType.Supper).Forget();
            }
            else if (upNumber >= 2 && leftNumber >= 2)
            {
                DeleteElementsOnDirection(cellPosition, Vector2Int.down, upNumber);
                DeleteElementsOnDirection(cellPosition, Vector2Int.left, leftNumber);
                DeleteElementAndReplace(cellPosition, CellType.Bomb).Forget();
            }
            else if (upNumber >= 2 && rightNumber >= 2)
            {
                DeleteElementsOnDirection(cellPosition, Vector2Int.down, upNumber);
                DeleteElementsOnDirection(cellPosition, Vector2Int.right, rightNumber);
                DeleteElementAndReplace(cellPosition, CellType.Bomb).Forget();
            }
            else if (downNumber >= 2 && leftNumber >= 2)
            {
                DeleteElementsOnDirection(cellPosition, Vector2Int.up, downNumber);
                DeleteElementsOnDirection(cellPosition, Vector2Int.left, leftNumber);
                DeleteElementAndReplace(cellPosition, CellType.Bomb).Forget();
            }
            else if (downNumber >= 2 && rightNumber >= 2)
            {
                DeleteElementsOnDirection(cellPosition, Vector2Int.up, downNumber);
                DeleteElementsOnDirection(cellPosition, Vector2Int.right, rightNumber);
                DeleteElementAndReplace(cellPosition, CellType.Bomb).Forget();
            }
            else if (rightNumber >= 1 && upNumber >= 1 && rightUpNumber >= 1)
            {
                DeleteElementsOnDirection(cellPosition, Vector2Int.down, 1);
                DeleteElementsOnDirection(cellPosition, Vector2Int.right, 1);
                DeleteElementsOnDirection(cellPosition, Vector2Int.right + Vector2Int.down, 1);
                DeleteElementAndReplace(cellPosition, CellType.LightningBolt).Forget();
            }
            else if (rightNumber >= 1 && downNumber >= 1 && rightDownNumber >= 1)
            {
                DeleteElementsOnDirection(cellPosition, Vector2Int.up, 1);
                DeleteElementsOnDirection(cellPosition, Vector2Int.right, 1);
                DeleteElementsOnDirection(cellPosition, Vector2Int.right + Vector2Int.up, 1);
                DeleteElementAndReplace(cellPosition, CellType.LightningBolt).Forget();
            }
            else if (leftNumber >= 1 && upNumber >= 1 && leftUpNumber >= 1)
            {
                DeleteElementsOnDirection(cellPosition, Vector2Int.down, 1);
                DeleteElementsOnDirection(cellPosition, Vector2Int.left, 1);
                DeleteElementsOnDirection(cellPosition, Vector2Int.left + Vector2Int.down, 1);
                DeleteElementAndReplace(cellPosition, CellType.LightningBolt).Forget();
            }
            else if (leftNumber >= 1 && downNumber >= 1 && leftDownNumber >= 1)
            {
                DeleteElementsOnDirection(cellPosition, Vector2Int.up, 1);
                DeleteElementsOnDirection(cellPosition, Vector2Int.left, 1);
                DeleteElementsOnDirection(cellPosition, Vector2Int.left + Vector2Int.up, 1);
                DeleteElementAndReplace(cellPosition, CellType.LightningBolt).Forget();
            }
            else if (rightNumber + leftNumber >= 2)
            {
                DeleteElementsOnDirection(cellPosition, Vector2Int.right, rightNumber);
                DeleteElementsOnDirection(cellPosition, Vector2Int.left, leftNumber);
                ExplodeCellAsync(cellPosition).Forget();
            }
            else if (upNumber + downNumber >= 2)
            {
                DeleteElementsOnDirection(cellPosition, Vector2Int.down, upNumber);
                DeleteElementsOnDirection(cellPosition, Vector2Int.up, downNumber);
                ExplodeCellAsync(cellPosition).Forget();
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
                {
                    if (_map[j, i] != null)
                        _map[j, i].SetPause(isPause);
                }
            }

            OnPause?.Invoke(isPause);
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

        private void DeleteElementsOnDirection(Vector2Int cellPosition, Vector2Int direction, int lenght)
        {
            for (int i = 1; i <= lenght; i++)
                ExplodeCellAsync(cellPosition + direction * i).Forget();
        }
        private async UniTaskVoid DeleteElementAndReplace(Vector2Int cellPosition, CellType newElementType)
        {
            await ExplodeCellAsync(cellPosition);
            _map[cellPosition.y, cellPosition.x] =
                    _cellFabric.GetCell(newElementType, CellPositionToWorld(cellPosition), Quaternion.identity, _cellContainer);
        }

        private void TryFillBoard()
        {
            if (!_isBoardFillUp)
                 FillBoardAsync().Forget();
        }
        private async UniTask FillBoardAsync()
        {
            if (_isBoardFillUp)
                return;

            _isBoardFillUp = true;
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
                            if (_map[j, i].IsStatic )
                            {
                                if(j == 0)
                                    continue;

                                spawnQueue = 0;
                                lowerElementIndex = 0;
                                continue;
                            }

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
                        _map[j, i] = _cellFabric.GetCell(GetRandomElementType(), pos, Quaternion.identity, _cellContainer);
                        _map[j, i].MoveTo(CellPositionToWorld(new Vector2Int(i, j)), true, DoCallback);
                        areElementsMoved = false;
                        spawnQueue--;
                    }
                }

                if (_needHandleCells)
                {
                    _needHandleCells = false;
                    for (int i = 0; i < _horizontalMapSize; i++)
                    {
                        for (int j = 0; j < _verticalMapSize; j++)
                        {
                            if (_cellHandlingMap[j, i] == true)
                            {
                                _cellHandlingMap[j, i] = false;
                                bool handled = HandleMove(new Vector2Int(i, j));
                                if (handled == true)
                                    areElementsMoved = false;
                            }
                        }
                    }
                }

                await UniTask.Yield(this.GetCancellationTokenOnDestroy(), true);
            } while (!areElementsMoved);

            _isBoardFillUp = false;

            void DoCallback(Cell cell)
            {
                Vector2Int position = WorldPositionToCell(cell.transform.position);
                if(IsPositionInBoard(position + Vector2Int.up + Vector2Int.right) && _map[position.y + 1, position.x + 1] == null)
                {
                    Debug.Log("Move right");
                    _map[position.y, position.x] = null;
                    _map[position.y + 1, position.x + 1] = cell;
                    _map[position.y + 1, position.x + 1].MoveTo(CellPositionToWorld(position + Vector2Int.up + Vector2Int.right), true, DoCallback);
                }
                else if(IsPositionInBoard(position + Vector2Int.up + Vector2Int.left) && _map[position.y + 1, position.x - 1] == null)
                {
                    Debug.Log("Move left");
                    _map[position.y, position.x] = null;
                    _map[position.y + 1, position.x - 1] = cell;
                    _map[position.y + 1, position.x - 1].MoveTo(CellPositionToWorld(position + Vector2Int.up + Vector2Int.left), true, DoCallback);
                }
                else
                {
                    Debug.Log("Handle");
                    _cellHandlingMap[position.y, position.x] = true;
                    _needHandleCells = true;
                }
            }
        }
        private void HandleAbilityCallback(IAbility ability)
        {
            _usedAbilities.Remove(ability);
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
        private bool CanHandleCellForSwipe(Cell cell)
        {
            return !(cell.IsMove || cell.IsExplode || cell.IsStatic);
        }
        private bool CanHandleCellForGetScore(Vector2Int cellPosition, CellType type)
        {
            if (!IsPositionInBoard(cellPosition))
                return false;

            Cell cell = GetCell(cellPosition);
            return CanHandleCellForSwipe(cell) && cell.Type == type;
        }
    }
}
