using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Service.Saving;
using Core.Infrastructure.UI;
using Core.Infrastructure.Gameplay;
using Core.Infrastructure.Service;

namespace Core.UI.Gameplay
{
    public class PausePopup : WindowBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _quitButton;
        [Header("Switched Button")]
        [SerializeField] private SwitchButton _musicButton;
        [SerializeField] private SwitchButton _soundButton;
        [Header("Audio Keys")]
        [SerializeField] private ClipEvent _uiClickKey;
        [SerializeField] private ClipEvent _uiSwitchKey;

        private IAudioService _audioService;
        private IGameModeSimulation _gameModeSimulation;
        private ILevelSceneSimulation _levelSceneSimulation;
        private ILevelService _levelService;

        public override event Action OnMenuBack;

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
            _closeButton.onClick.AddListener(BackMenu);
            _closeButton.onClick.AddListener(ClickSound);
            _restartButton.onClick.AddListener(RestartLevel);
            _restartButton.onClick.AddListener(ClickSound);
            _quitButton.onClick.AddListener(QuitToMainMenu);
            _quitButton.onClick.AddListener(ClickSound);
            _musicButton.Button.onClick.AddListener(SwitchBackgroundMusicVolume);
            _musicButton.Button.onClick.AddListener(SwitchSound);
            _soundButton.Button.onClick.AddListener(SwitchSoundsVolume);
            _soundButton.Button.onClick.AddListener(SwitchSound);

            _musicButton.SetActive(!(_audioService.GetVolume(AudioGroupType.Music) < 0f));
            _soundButton.SetActive(!(_audioService.GetVolume(AudioGroupType.Sound) < 0f));
        }
        protected override void OnHide()
        {
            _closeButton.onClick.RemoveAllListeners();
            _restartButton.onClick.RemoveAllListeners();
            _quitButton.onClick.RemoveAllListeners();
            _musicButton.Button.onClick.RemoveAllListeners();
            _soundButton.Button.onClick.RemoveAllListeners();
        }
        protected override void OnFocus()
        {
            _closeButton.interactable = true;
            _quitButton.interactable = true;
            _restartButton.interactable = true;
            _musicButton.Button.interactable = true;
            _soundButton.Button.interactable = true;
        }
        protected override void OnUnfocus()
        {
            _closeButton.interactable = false;
            _quitButton.interactable = false;
            _restartButton.interactable = false;
            _musicButton.Button.interactable = false;
            _soundButton.Button.interactable = false;
        }
        protected override void OnClose()
        {
            OnHide();
            OnUnfocus();
        }

        private void BackMenu()
        {
            OnMenuBack?.Invoke();
        }

        private void QuitToMainMenu()
        {
            _audioService.ChangeSnapshot(AudioSnapshotType.Default);
            _gameModeSimulation.HandleEndGame();
            _levelService.ResetLevelConfig();
            _levelSceneSimulation.QuitToMainMenu();
        }
        private void RestartLevel()
        {
            _audioService.ChangeSnapshot(AudioSnapshotType.Default);
            _gameModeSimulation.HandleEndGame();
            _levelSceneSimulation.RestartLevel();
        }

        private void SwitchBackgroundMusicVolume()
        {
            float nextVolumeValue;
            if (_audioService.GetVolume(AudioGroupType.Music) < 0f)
                nextVolumeValue = 0f;
            else
                nextVolumeValue = -80f;

            _audioService.SetVolume(AudioGroupType.Music, nextVolumeValue);

            PlayerPrefs.SetFloat(PlayerPrefsEnum.AudioSettings.MUSIC_VOLUME_VALUE_KEY, nextVolumeValue);
            PlayerPrefs.Save();

            _musicButton.Switch();
        }
        private void SwitchSoundsVolume()
        {
            float nextVolumeValue;
            if (_audioService.GetVolume(AudioGroupType.Sound) < 0f)
                nextVolumeValue = 0f;
            else
                nextVolumeValue = -80f;

            _audioService.SetVolume(AudioGroupType.Sound, nextVolumeValue);

            PlayerPrefs.SetFloat(PlayerPrefsEnum.AudioSettings.SOUND_VOLUME_VALUE_KEY, nextVolumeValue);
            PlayerPrefs.Save();

            _soundButton.Switch();
        }

        private void ClickSound()
        {
            _audioService.PlayOneShot(_uiClickKey);
        }
        private void SwitchSound()
        {
            _audioService.PlayOneShot(_uiSwitchKey);
        }
    }
}
