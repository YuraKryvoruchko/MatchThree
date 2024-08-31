using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using Core.Infrastructure.Service.Audio;
using Core.Infrastructure.UI;

namespace Core.UI.Gameplay
{
    public class CompletePopup : WindowBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _quitButton;
        [Header("Result Panel Settings")]
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private float _scoreAccumulationTime;
        [Space]
        [SerializeField] private StarSettings[] _stars;
        [SerializeField] private float _starShowingDelay;
        [Header("Audio Keys")]
        [SerializeField] private ClipEvent _uiClickKey;

        private IAudioService _audioService;

        public override event Action OnMenuBack;

        [Serializable]
        private class StarSettings
        {
            public GameObject Image;
            public float MinProgress;
        }

        [Inject]
        private void Construct(IAudioService audioService)
        {
            _audioService = audioService;
        }

        protected override void OnShow()
        {
            _nextButton.onClick.AddListener(ClickSound);
            _nextButton.onClick.AddListener(LoadNextLevel);
            _quitButton.onClick.AddListener(ClickSound);
            _quitButton.onClick.AddListener(LoadMainMenu);
        }
        protected override void OnHide()
        {
            _nextButton.onClick.RemoveAllListeners();
            _quitButton.onClick.RemoveAllListeners();
        }
        protected override void OnFocus()
        {
            _nextButton.interactable = true;
            _quitButton.interactable = true;
        }
        protected override void OnUnfocus()
        {
            _nextButton.interactable = false;
            _quitButton.interactable = false;
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
            Debug.LogWarning("Next level loading is not implemented!", this);
        }
        private void LoadMainMenu()
        {
            Debug.LogWarning("Main Menu loading is not implemented!", this);
        }

        private void ClickSound()
        {
            _audioService.PlayOneShot(_uiClickKey);
        }
    }
}
