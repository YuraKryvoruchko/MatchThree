using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using Zenject;
using Core.Infrastructure.Service;
using Core.Infrastructure.UI;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.Loading;
using System;

namespace Core.UI
{
    public class SelectLevelMenu : WindowBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _backButton;
        [Header("Scene Keys")]
        [SerializeField] private AssetReferenceGameObject _levelSelectionButtonReference;
        [Header("Audio Keys")]
        [SerializeField] private ClipEvent _clickAudioPath;

        private ILoadingScreenProvider _loadingScreenProvider;

        private SceneService _sceneService;
        private IAudioService _audioService;
        private ILevelService _levelService;

        public override event Action OnMenuBack;

        [Inject]
        private void Construct(SceneService sceneService, IAudioService audioService, ILevelService levelService, 
            ILoadingScreenProvider loadingScreenProvider)
        {
            _sceneService = sceneService;
            _audioService = audioService;
            _levelService = levelService;
            _loadingScreenProvider = loadingScreenProvider;
        }

        protected override void OnShow()
        {
            throw new NotImplementedException();
        }
        protected override void OnHide()
        {
            throw new NotImplementedException();
        }
        protected override void OnFocus()
        {
            throw new NotImplementedException();
        }
        protected override void OnUnfocus()
        {
            throw new NotImplementedException();
        }
        protected override void OnClose()
        {
            throw new NotImplementedException();
        }
    }
}
