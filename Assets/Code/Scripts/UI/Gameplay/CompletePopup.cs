using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.UI;
using Core.Infrastructure.Gameplay;
using Core.Infrastructure.Service;

namespace Core.UI.Gameplay
{
    public class CompletePopup : WindowBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _restartLevelButton;
        [Header("Result Panel Settings")]
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private float _scoreAccumulationTime;
        [Space]
        [SerializeField] private StarSettings[] _stars;
        [SerializeField] private float _starShowingDelay;
        [Header("Audio Keys")]
        [SerializeField] private ClipEvent _uiClickKey;

        private IAudioService _audioService;
        private IGameModeSimulation _gameModeSimulation;
        private ILevelSceneSimulation _levelSceneSimulation;
        private ILevelService _levelService;

        public override event Action OnMenuBack;

        [Serializable]
        private class StarSettings
        {
            public GameObject Image;
            public float MinProgress;
        }

        [Inject]
        private void Construct(IAudioService audioService, IGameModeSimulation gameModeSimulation,
            ILevelSceneSimulation levelSceneSimulation, ILevelService levelService)
        {
            _audioService = audioService;
            _gameModeSimulation = gameModeSimulation;
            _levelSceneSimulation = levelSceneSimulation;
            _levelService = levelService;
        }

        protected override void OnShow()
        {
            _nextLevelButton.onClick.AddListener(ClickSound);
            _nextLevelButton.onClick.AddListener(LoadNextLevel);
            _restartLevelButton.onClick.AddListener(ClickSound);
            _restartLevelButton.onClick.AddListener(RestartLevel);
        }
        protected override void OnHide()
        {
            _nextLevelButton.onClick.RemoveAllListeners();
            _restartLevelButton.onClick.RemoveAllListeners();
        }
        protected override void OnFocus()
        {
            _nextLevelButton.interactable = true;
            _restartLevelButton.interactable = true;
        }
        protected override void OnUnfocus()
        {
            _nextLevelButton.interactable = false;
            _restartLevelButton.interactable = false;
        }
        protected override void OnClose()
        {
            OnHide();
            OnUnfocus();
        }

        public async UniTaskVoid Activate(float progress, int scoreCount)
        {
            DOTween.To((value) => _scoreText.text = ((int)Mathf.Lerp(0, scoreCount, value)).ToString(), 0, 1, _scoreAccumulationTime);

            for(int i = 0; i < _stars.Length; i++)
            {
                if (_stars[i].MinProgress > progress)
                    break;

                _stars[i].Image.SetActive(true);

                await UniTask.WaitForSeconds(_starShowingDelay);
            }
        }

        private void LoadNextLevel()
        {
            if (_levelService.LevelConfigCount - 1 == _levelService.CurentLevelConfigIndex)
                QuitToMainMenu();

            _levelService.SetCurrentLevelConfigByIndex(_levelService.CurentLevelConfigIndex + 1);
            _gameModeSimulation.HandleEndGame();
            _levelSceneSimulation.RestartLevel();
        }
        private void RestartLevel()
        {
            _gameModeSimulation.HandleEndGame();
            _levelSceneSimulation.RestartLevel();
        }
        private void QuitToMainMenu()
        {
            _gameModeSimulation.HandleEndGame();
            _levelService.ResetLevelConfigIndex();
            _levelSceneSimulation.QuitToMainMenu();
        }

        private void ClickSound()
        {
            _audioService.PlayOneShot(_uiClickKey);
        }
    }
}
