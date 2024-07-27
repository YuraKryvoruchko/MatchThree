using UnityEngine;
using Zenject;
using Core.Infrastructure.Service;

namespace Core.Gameplay
{
    public class GameplayLevelSetup : MonoBehaviour
    {
        [SerializeField] private AudioPath _backgroundAudioPath;

        private AudioService _audioService;

        [Inject]
        private void Construct(AudioService audioService)
        {
            _audioService = audioService;
            _audioService.Play(_backgroundAudioPath);
        }
    }
}
