using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Core.Gameplay.Input;
using Core.Infrastructure.Gameplay;
using Core.Infrastructure.Service;
using Core.UI.Gameplay;

namespace Core.Gameplay
{
    public class AbilityThrowMode : IInitializable
    {
        private BoardClickDetection _cellClickDetection;

        private CellType _abilityType;

        private IReadOnlyCollection<HolderAbilitySettings> _abilitySettings;

        private readonly GameField _gameField;

        private readonly Dictionary<CellType, int> _abilityCount;

        private readonly IGameModeSimulation _gameModeSimulation;
        private readonly ILevelService _levelService;

        public bool IsActive { get; private set; }

        public event Action OnEnableMode;
        public event Action OnDisableMode;
        public event Action<CellType, int> OnUse;

        public AbilityThrowMode(IGameModeSimulation gameModeSimulation, ILevelService levelService,
            GameField gameField, BoardClickDetection cellClickDetection)
        {
            _gameModeSimulation = gameModeSimulation;
            _gameModeSimulation.OnBlockGame += HandleGameComplete;

            _levelService = levelService;

            _gameField = gameField;
            _cellClickDetection = cellClickDetection;

            _abilityCount = new Dictionary<CellType, int>();
        }
        void IInitializable.Initialize()
        {
            _gameModeSimulation.OnBlockGame += HandleGameComplete;

            LevelConfig config = _levelService.GetCurrentLevelConfig();
            _abilitySettings = config.AbilitySettings;
            _abilityCount.EnsureCapacity(_abilitySettings.Count);
            foreach(HolderAbilitySettings settings in _abilitySettings)
            {
                _abilityCount.Add(settings.AbilityType, settings.Amount);
            }
        }

        public void HandleClickOnBoard(Vector3 worldClickPosition)
        {
            Vector2Int cellPosition = _gameField.WorldPositionToCell(worldClickPosition);
            _gameField.UseAbility(_abilityType, cellPosition, cellPosition);
            _abilityCount[_abilityType]--;

            OnUse?.Invoke(_abilityType, _abilityCount[_abilityType]);

            DisableAbilityThrowMode();
        }
        public void EnableAbilityThrowMode(CellType abilityType)
        {
            _abilityType = abilityType;
            _cellClickDetection.OnBoardClick += HandleClickOnBoard;
            IsActive = true;
            OnEnableMode?.Invoke();
        }
        public void DisableAbilityThrowMode()
        {
            _cellClickDetection.OnBoardClick -= HandleClickOnBoard;
            IsActive = false;
            OnDisableMode?.Invoke();
        }
        public void ChangeAbility(CellType abilityType)
        {
            _abilityType = abilityType;
        }
        public bool CanUseAbility(CellType abilityType)
        {
            return _abilityCount[abilityType] > 0;
        }
        public IReadOnlyDictionary<CellType, int> GetAbilityCountDictionary()
        {
            return _abilityCount;
        }
        public IReadOnlyCollection<HolderAbilitySettings> GetHolderAbilitySettings()
        {
            return _abilitySettings;
        }

        private void HandleGameComplete()
        {
            _gameModeSimulation.OnBlockGame -= HandleGameComplete;
            DisableAbilityThrowMode();
        }
    }
}
