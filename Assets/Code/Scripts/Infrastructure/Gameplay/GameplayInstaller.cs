using UnityEngine;
using Zenject;
using Core.Input;
using Core.Gameplay.Input;
using Core.Infrastructure.Factories;
using Core.Gameplay;
using Core.UI;
using Core.Infrastructure.Service.Pause;
using UnityEngine.AddressableAssets;
using Core.Infrastructure.Service.Saving;

namespace Core.Infrastructure.Gameplay
{
    public class GameplayInstaller : MonoInstaller
    {
        [Header("Mode")]
#if UNITY_EDITOR
        [SerializeField] private bool _setInInspector;
#endif
        [SerializeField] private bool _isLevelMode;
        [Header("Game Objects")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private GameField _gameField;
        [Header("Startups\nUI")]
        [SerializeField] private Transform _uiContainer;
        [SerializeField] private AssetReferenceGameObject _longModeMenu;
        [SerializeField] private AssetReferenceGameObject _levelModeMenu;
        [Header("Scene")]
        [SerializeField] private AssetReference _gameScene;
        [SerializeField] private AssetReference _mainScene;
        [Header("Input")]
        [SerializeField] private SwipeDetection _swipeDetection;
        [Header("Configs\nFabrics")]
        [SerializeField] private FieldCellFabric.FieldCellFabricConfig _cellFabricConfig;
        [SerializeField] private AbilityFactory.AbilityFactoryConfig _abilityFactoryConfig;

        public override void InstallBindings()
        {
#if UNITY_EDITOR
            if(!_setInInspector)
#endif
                _isLevelMode = PlayerPrefs.GetInt(PlayerPrefsEnum.GameModeSettings.IS_LEVEL_MODE_VALUE, 0) > 0;

            BindPauseServiceAndPauseProvider();
            BindCellFabric();
            BindAbilityFabric();

            BindSwipeDetection();
            BindCellClickDetection();
            BindCellSwipeDetection();

            BindGameField();
            BindGameScoreTracking();
            BindPlayerMoveTracking();

            BindLevelTaskCompletionChecker();
            BindGameModeSimulation();
            BindLevelSceneSimulation();

            BindAbilityThrowMode();

            BindGameplayUIStartup();
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
                .BindInterfacesAndSelfTo<BoardClickDetection>()
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
                .BindInterfacesAndSelfTo<AbilityThrowMode>()
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
                    .BindInterfacesTo<UIStartup>()
                    .AsSingle()
                    .WithArguments(_levelModeMenu, _uiContainer);
            }
            else
            {
                Container
                    .BindInterfacesTo<UIStartup>()
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
        private void BindLevelSceneSimulation()
        {
            Container
                .Bind<ILevelSceneSimulation>()
                .To<LevelSceneSimulation>()
                .AsSingle()
                .WithArguments(_gameScene, _mainScene);
        }
    }
}
