using UnityEngine;
using Zenject;
using Core.Infrastructure.Service.Audio;

namespace Core.Gameplay
{
    public class GameplayLevelSetup : MonoBehaviour
    {
        [SerializeField] private ClipEvent _backgroundAudioPath;

        private IAudioService _audioService;

        [Inject]
        private void Construct(IAudioService audioService)
        {
            _audioService = audioService;
            _audioService.PlayWithSource(_backgroundAudioPath);
        }
    }
}
