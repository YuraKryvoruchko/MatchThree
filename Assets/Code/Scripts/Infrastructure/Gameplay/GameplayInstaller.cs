﻿using UnityEngine;
using Zenject;
using Core.Input;
using Core.Gameplay.Input;
using Core.Infrastructure.Factories;
using Core.Gameplay;
using Core.UI.Gameplay;
using Core.Infrastructure.Service.Pause;
using UnityEngine.AddressableAssets;

namespace Core.Infrastructure.Gameplay
{
    public class GameplayInstaller : MonoInstaller
    {
        [Header("Mode")]
        [SerializeField] private bool _isLevelMode;
        [Header("Game Objects")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private GameField _gameField;
        [Header("Startups\nUI")]
        [SerializeField] private Transform _uiContainer;
        [SerializeField] private AssetReferenceGameObject _longModeMenu;
        [SerializeField] private AssetReferenceGameObject _levelModeMenu;
        [Header("Input")]
        [SerializeField] private SwipeDetection _swipeDetection;
        [Header("Configs\nFabrics")]
        [SerializeField] private FieldCellFabric.FieldCellFabricConfig _cellFabricConfig;
        [SerializeField] private AbilityFactory.AbilityFactoryConfig _abilityFactoryConfig;

        public override void InstallBindings()
        {
            BindPauseServiceAndPauseProvider();
            BindCellFabric();
            BindAbilityFabric();
            BindSwipeDetection();
            BindCellClickDetection();
            BindCellSwipeDetection();
            BindGameField();
            BindAbilityThrowMode();
            BindGameScoreTracking();
            BindPlayerMoveTracking();
            BindGameplayUIStartup();

            BindLevelTaskCompletionChecker();
            BindGameModeSimulation();
        }

        private void BindPauseServiceAndPauseProvider()
        {
            Container
                .Bind(typeof(IPauseService), typeof(IPauseProvider))
                .To<PauseService>()
                .AsSingle();
        }
        private void BindCellFabric()
        {
            Container
                .Bind<ICellFabric>()
                .To<FieldCellFabric>()
                .AsSingle()
                .WithArguments(_cellFabricConfig);
        }
        private void BindAbilityFabric()
        {
            Container
                .Bind<IAbilityFactory>()
                .To<AbilityFactory>()
                .AsSingle()
                .WithArguments(_abilityFactoryConfig);
        }
        private void BindSwipeDetection()
        {
            Container
                .Bind<SwipeDetection>()
                .FromInstance(_swipeDetection)
                .AsSingle();
        }
        private void BindCellClickDetection()
        {
            Container
                .BindInterfacesAndSelfTo<CellClickDetection>()
                .AsSingle()
                .WithArguments(_mainCamera);
        }
        private void BindCellSwipeDetection()
        {
            Container
                .BindInterfacesAndSelfTo<CellSwipeDetection>()
                .AsSingle()
                .WithArguments(_mainCamera);
        }
        private void BindGameField()
        {
            Container
                .Bind<GameField>()
                .FromInstance(_gameField)
                .AsSingle();
        }
        private void BindAbilityThrowMode()
        {
            Container
                .Bind<AbilityThrowMode>()
                .AsSingle();
        }
        private void BindPlayerMoveTracking()
        {
            Container
                .BindInterfacesAndSelfTo<PlayerMoveTracking>()
                .AsSingle();
        }
        private void BindGameScoreTracking()
        {
            Container
                .BindInterfacesAndSelfTo<GameScoreTracking>()
                .AsSingle();
        }
        private void BindGameplayUIStartup()
        {
            if (_isLevelMode)
            {
                Container
                    .BindInterfacesTo<GameplayUIStartup>()
                    .AsSingle()
                    .WithArguments(_levelModeMenu, _uiContainer);
            }
            else
            {
                Container
                    .BindInterfacesTo<GameplayUIStartup>()
                    .AsSingle()
                    .WithArguments(_longModeMenu, _uiContainer);
            }
        }
        private void BindLevelTaskCompletionChecker()
        {
            if (!_isLevelMode)
                return;

            Container
                .BindInterfacesAndSelfTo<LevelTaskCompletionChecker>()
                .AsSingle();
        }
        private void BindGameModeSimulation()
        {
            if (_isLevelMode)
            {
                Container
                    .BindInterfacesTo<LevelModeSimulation>()
                    .AsSingle();
            }
            else
            {
                Container
                    .BindInterfacesTo<LongModeSimulation>()
                    .AsSingle();
            }
        }
    }
}
