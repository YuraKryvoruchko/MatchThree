using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using Zenject;
using Cysharp.Threading.Tasks;
using Core.Infrastructure.Service;
using Code.Infrastructure.Loading;
using System;
using UnityEditor.Search;
using System.Collections;
using System.Collections.Generic;

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
        [SerializeField] private AudioPath _clickAudioPath;
        [SerializeField] private AudioFile[] _gameplayLoadingAudioFiles;

        private ILoadingScreenProvider _loadingScreenProvider;

        private SceneService _sceneService;
        private AudioService _audioService;

        private class MainMenuMusicUnloadingOperation : ILoadingOperation
        {
            private AudioService _audioService;
            private AudioFileType[] _audioTypes;

            string ILoadingOperation.Description => "Unloading main menu audio...";

            public MainMenuMusicUnloadingOperation(AudioService audioService, AudioFileType[] types)
            {
                _audioService = audioService;
                _audioTypes = types;
            }

            async UniTask ILoadingOperation.Load(Action<float> onProgress)
            {
                float progressStep = 1f / _audioTypes.Length;
                for (int i = 0; i < _audioTypes.Length; i++)
                {
                    _audioService.UnLoadAudioFile(_audioTypes[i]);
                    onProgress?.Invoke(progressStep * (i + 1));
                }
            }
        }
        private class GameplaySceneLoadingOperation : ILoadingOperation
        {
            private AssetReference _gamePlayScene;
            private AssetReference _mainMenuScene;

            private SceneService _sceneService;

            string ILoadingOperation.Description => "Loading level...";

            public GameplaySceneLoadingOperation(SceneService sceneService, AssetReference gamePlayScene, AssetReference mainMenuScene)
            {
                _sceneService = sceneService;
                _gamePlayScene = gamePlayScene;
                _mainMenuScene = mainMenuScene;
            }

            async UniTask ILoadingOperation.Load(Action<float> onProgress)
            {
                await _sceneService.UnloadSceneAsync(_mainMenuScene.AssetGUID);
                onProgress?.Invoke(0.6f);

                var gamePlayScene = await _sceneService.LoadSceneAsync(_gamePlayScene.AssetGUID,
                    UnityEngine.SceneManagement.LoadSceneMode.Additive);
                onProgress?.Invoke(1f);
            }
        }
        private class GameplayAudioLoadingOperation : ILoadingOperation
        {
            private AudioService _audioService;
            private AudioFile[] _audioFiles;

            string ILoadingOperation.Description => "Loading audio...";

            public GameplayAudioLoadingOperation(AudioService audioService, AudioFile[] files)
            {
                _audioService = audioService;
                _audioFiles = files;
            }

            async UniTask ILoadingOperation.Load(Action<float> onProgress)
            {
                float progressStep = 1f / _audioFiles.Length;
                for (int i = 0; i < _audioFiles.Length; i++)
                {
                    await _audioService.LoadAudioFile(_audioFiles[i]);
                    onProgress?.Invoke(progressStep * (i + 1));
                }
            }
        }

        [Inject]
        private void Construct(SceneService sceneService, AudioService audioService, ILoadingScreenProvider loadingScreenProvider)
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
            Queue<ILoadingOperation> queue = new Queue<ILoadingOperation>(3);
            queue.Enqueue(new MainMenuMusicUnloadingOperation(_audioService, new AudioFileType[] { AudioFileType.Background }));
            queue.Enqueue(new GameplayAudioLoadingOperation(_audioService, _gameplayLoadingAudioFiles));
            queue.Enqueue(new GameplaySceneLoadingOperation(_sceneService, _gamePlayScene, _mainMenuScene));
            await _loadingScreenProvider.LoadAndDestroy(queue);
        }
    }
}
