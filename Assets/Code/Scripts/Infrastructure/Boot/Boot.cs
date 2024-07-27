using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Zenject;
using Core.Infrastructure.Service;
using Code.Infrastructure.Loading;

namespace Core.Infrastructure.Boot
{
    public class Boot : MonoBehaviour, IInitializable
    {
        [SerializeField] private AssetReference _mainMenuSceneReference;
        [SerializeField] private AudioFile[] _mainMenuAudio;

        private SceneService _sceneService;
        private AudioService _audioService;

        private ILoadingScreenProvider _loadingScreenProvider;

        private class MainMenuSceneLoadingOperation : ILoadingOperation
        {
            private AssetReference _mainMenuScene;

            private SceneService _sceneService;

            string ILoadingOperation.Description => "Loading the main menu...";

            public MainMenuSceneLoadingOperation(SceneService sceneService, AssetReference mainMenuScene)
            {
                _sceneService = sceneService;
                _mainMenuScene = mainMenuScene;
            }

            async UniTask ILoadingOperation.Load(Action<float> onProgress)
            {
                onProgress?.Invoke(0.3f);
                var gamePlayScene = await _sceneService.LoadSceneAsync(_mainMenuScene.AssetGUID,
                    UnityEngine.SceneManagement.LoadSceneMode.Additive);
                onProgress?.Invoke(1f);
            }
        }
        private class MainMenuAudioLoadingOperation : ILoadingOperation
        {
            private AudioService _audioService;
            private AudioFile[] _audioFiles;

            string ILoadingOperation.Description => "Loading audio...";

            public MainMenuAudioLoadingOperation(AudioService audioService, AudioFile[] files)
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

        public async void Initialize()
        {
            await Addressables.InitializeAsync();
            Queue<ILoadingOperation> queue = new Queue<ILoadingOperation>(2);
            queue.Enqueue(new MainMenuAudioLoadingOperation(_audioService, _mainMenuAudio));
            queue.Enqueue(new MainMenuSceneLoadingOperation(_sceneService, _mainMenuSceneReference));
            await _loadingScreenProvider.LoadAndDestroy(queue);
        }
    }
}
