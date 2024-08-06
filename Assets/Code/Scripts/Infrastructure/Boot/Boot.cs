using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Zenject;
using Core.Infrastructure.Service;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Loading;

namespace Core.Infrastructure.Boot
{
    public class Boot : MonoBehaviour, IInitializable
    {
        [SerializeField] private AssetReference _mainMenuSceneReference;
        [SerializeField] private ClipEvent[] _mainMenuAudio;

        private SceneService _sceneService;
        private IAudioService _audioService;

        private ILoadingScreenProvider _loadingScreenProvider;

        private class MainMenuAudioLoadingOperation : ILoadingOperation
        {
            private AudioService _audioService;
            private ClipEvent[] _audioFiles;

            string ILoadingOperation.Description => "Loading audio...";

            public MainMenuAudioLoadingOperation(AudioService audioService, ClipEvent[] files)
            {
                _audioService = audioService;
                _audioFiles = files;
            }

            async UniTask ILoadingOperation.Load(Action<float> onProgress)
            {
                //float progressStep = 1f / _audioFiles.Length;
                //for (int i = 0; i < _audioFiles.Length; i++)
                //{
                //    await _audioService.LoadAudioFile(_audioFiles[i]);
                //    onProgress?.Invoke(progressStep * (i + 1));
                //}
            }
        }

        [Inject]
        private void Construct(SceneService sceneService, IAudioService audioService, ILoadingScreenProvider loadingScreenProvider)
        {
            _sceneService = sceneService;
            _audioService = audioService;
            _loadingScreenProvider = loadingScreenProvider;
        }

        public async void Initialize()
        {
            await Addressables.InitializeAsync();
            Queue<ILoadingOperation> queue = new Queue<ILoadingOperation>(2);
            //queue.Enqueue(new MainMenuAudioLoadingOperation(_audioService, _mainMenuAudio));
            queue.Enqueue(new SceneLoadingOperation(_sceneService, _mainMenuSceneReference));
            await _loadingScreenProvider.LoadAndDestroy(queue);
        }
    }
}
