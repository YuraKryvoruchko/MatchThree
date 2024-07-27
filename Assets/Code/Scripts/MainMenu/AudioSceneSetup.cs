using UnityEngine;
using Zenject;
using Core.Infrastructure.Service;

namespace Core.MainMenu
{
    public class AudioSceneSetup : MonoBehaviour
    {
        [SerializeField] private AudioPath _path;

        private AudioService _audioService;

        [Inject]
        private void Construct(AudioService audioService)
        {
            _audioService = audioService;
            _audioService.Play(_path);
        }
    }
}
