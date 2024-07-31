using com.cyborgAssets.inspectorButtonPro;
using UnityEngine;
using Zenject;

namespace Assets.Code.Scripts.Infrastructure.Services.AudioService.Version
{
    public class OneShotClick : MonoBehaviour
    {
        [SerializeField] private ClipEvent _clickEvent;

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
            _audioService.PlayOneShot(_clickEvent);
        }
    }
}
