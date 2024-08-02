using com.cyborgAssets.inspectorButtonPro;
using UnityEngine;
using Zenject;

namespace Assets.Code.Scripts.Infrastructure.Services.AudioService.Version
{
    public class TestAbilityAudio : MonoBehaviour
    {
        [SerializeField] private ClipEvent _startAbility;
        [SerializeField] private ClipEvent _endAbility;

        private AudioService _audioService;

        private SourceInstance _sourceInstance;

        [Inject]
        private void Construct(AudioService audioService)
        {
            _audioService = audioService;
        }

        [ProPlayButton]
        private void StartPlay()
        {
            if (_sourceInstance == null)
                _sourceInstance = _audioService.PlayWithSource(_startAbility);
            else
                _sourceInstance.Play();
        }
        [ProPlayButton]
        private void Pause(bool isPause)
        {
            _sourceInstance.Pause(isPause);
        }
        [ProPlayButton]
        private void Continue()
        {
            _audioService.ReleaseSource(_sourceInstance);
            _audioService.PlayWithSource(_endAbility);
        }
        [ProPlayButton]
        private void Stop()
        {
            _sourceInstance.Stop();
        }
    }
}
