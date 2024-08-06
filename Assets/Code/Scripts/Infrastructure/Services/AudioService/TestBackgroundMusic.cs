using com.cyborgAssets.inspectorButtonPro;
using UnityEngine;
using Zenject;

namespace Core.Infrastructure.Service.Audio
{
    public class TestBackgroundMusic : MonoBehaviour
    {
        [SerializeField] private ClipEvent _backgroundEvent;

        private AudioService _audioService;
        private SourceInstance _sourceInstance;

        [Inject]
        private void Construct(AudioService audioService)
        {
            _audioService = audioService;
        }

        [ProPlayButton]
        private void Play()
        {
            if (_sourceInstance == null)
                _sourceInstance = _audioService.PlayWithSource(_backgroundEvent);
            else
                _sourceInstance.Play();
        }
        [ProPlayButton]
        private void Pause(bool isPause)
        {
            _sourceInstance.Pause(isPause);
        }
        [ProPlayButton]
        private void Stop()
        {
            _sourceInstance.Stop();
        }
        [ProPlayButton]
        private void Release()
        {
            _audioService.ReleaseSource(_sourceInstance);
            _sourceInstance = null;
        }
    }
}
