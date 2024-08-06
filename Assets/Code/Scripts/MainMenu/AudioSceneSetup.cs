using UnityEngine;
using Zenject;
using Core.Infrastructure.Service.Audio;

namespace Core.MainMenu
{
    public class AudioSceneSetup : MonoBehaviour
    {
        [SerializeField] private ClipEvent _path;

        private IAudioService _audioService;

        [Inject]
        private void Construct(IAudioService audioService)
        {
            _audioService = audioService;
            _audioService.PlayOneShot(_path);
        }
    }
}
