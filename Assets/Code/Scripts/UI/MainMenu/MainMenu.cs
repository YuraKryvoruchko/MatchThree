using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using Zenject;
using Cysharp.Threading.Tasks;
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
        [SerializeField] private ClipEvent[] _gameplayLoadingAudioFiles;

        private ILoadingScreenProvider _loadingScreenProvider;

        private SceneService _sceneService;
        private IAudioService _audioService;

        private class MainMenuMusicUnloadingOperation : ILoadingOperation
        {
            private IAudioService _audioService;
            private ClipEvent[] _audioTypes;

            string ILoadingOperation.Description => "Unloading main menu audio...";

            public MainMenuMusicUnloadingOperation(IAudioService audioService, ClipEvent[] types)
            {
                _audioService = audioService;
                _audioTypes = types;
            }

            async UniTask ILoadingOperation.Load(Action<float> onProgress)
            {
                //float progressStep = 1f / _audioTypes.Length;
                //for (int i = 0; i < _audioTypes.Length; i++)
                //{
                //    _audioService.UnLoadAudioFile(_audioTypes[i]);
                //    onProgress?.Invoke(progressStep * (i + 1));
                //}
            }
        }
        private class GameplayAudioLoadingOperation : ILoadingOperation
        {
            private IAudioService _audioService;
            private ClipEvent[] _audioFiles;

            string ILoadingOperation.Description => "Loading audio...";

            public GameplayAudioLoadingOperation(IAudioService audioService, ClipEvent[] files)
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
            //queue.Enqueue(new MainMenuMusicUnloadingOperation(_audioService, new AudioFileType[] { AudioFileType.Background }));
            //queue.Enqueue(new GameplayAudioLoadingOperation(_audioService, _gameplayLoadingAudioFiles));
            queue.Enqueue(new SceneUnloadingOperation(_sceneService, _mainMenuScene));
            queue.Enqueue(new SceneLoadingOperation(_sceneService, _gamePlayScene));
            await _loadingScreenProvider.LoadAndDestroy(queue);
        }
    }
}
