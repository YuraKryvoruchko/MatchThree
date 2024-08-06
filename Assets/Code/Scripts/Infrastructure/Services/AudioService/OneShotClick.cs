using com.cyborgAssets.inspectorButtonPro;
using UnityEngine;
using Zenject;

namespace Core.Infrastructure.Service.Audio
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
