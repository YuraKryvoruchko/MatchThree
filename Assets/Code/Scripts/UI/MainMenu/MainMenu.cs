using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using Zenject;
using Core.Infrastructure.Service;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Loading;

namespace Core.UI
{
    public class MainMenu : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _chooseLevelButton;
        [SerializeField] private Button _startLongModeButton;
        [SerializeField] private Button _settingsButton;
        [Header("Scene Keys")]
        [SerializeField] private AssetReference _mainMenuScene;
        [SerializeField] private AssetReference _gamePlayScene;
        [Header("Audio Keys")]
        [SerializeField] private ClipEvent _clickAudioPath;
        [Header("Loadable Audio")]
        [SerializeField] private ClipEvent[] _unloadingAudioList;
        [SerializeField] private ClipEvent[] _loadingAudioList;

        private ILoadingScreenProvider _loadingScreenProvider;

        private SceneService _sceneService;
        private IAudioService _audioService;

        [Inject]
        private void Construct(SceneService sceneService, IAudioService audioService, ILoadingScreenProvider loadingScreenProvider)
        {
            _sceneService = sceneService;
            _audioService = audioService;
            _loadingScreenProvider = loadingScreenProvider;
        }

        private void Awake()
        {
            _startLongModeButton.onClick.AddListener(LoadLongMode);
            _startLongModeButton.onClick.AddListener(() => _audioService.PlayOneShot(_clickAudioPath));
        }
        private void OnDestroy()
        {
            _startLongModeButton.onClick.RemoveAllListeners();
        }

        private async void LoadLongMode()
        {
            Queue<ILoadingOperation> queue = new Queue<ILoadingOperation>(4);
            queue.Enqueue(new AudioListUnloadingOperation(_unloadingAudioList));
            queue.Enqueue(new SceneUnloadingOperation(_sceneService, _mainMenuScene));
            queue.Enqueue(new AudioListLoadingOperation(_loadingAudioList));
            queue.Enqueue(new SceneLoadingOperation(_sceneService, _gamePlayScene));
            await _loadingScreenProvider.LoadAndDestroy(queue);
        }
    }
}
