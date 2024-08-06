using UnityEngine;
using Zenject;
using Core.Infrastructure.Service.Audio;

namespace Core.MainMenu
{
    public class BackgroundMusic : MonoBehaviour
    {
        [SerializeField] private ClipEvent _event;

        private IAudioService _audioService;
        private SourceInstance _sourceInstance;

        [Inject]
        private void Construct(IAudioService audioService)
        {
            _audioService = audioService;
            _sourceInstance = _audioService.PlayWithSource(_event);
        }

        private void OnDestroy()
        {
            _audioService.ReleaseSource(_sourceInstance);
        }
    }
}
