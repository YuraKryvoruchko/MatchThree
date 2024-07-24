using UnityEngine;
using Zenject;
using Core.Infrastructure.Service;

namespace Core.Gameplay
{
    public class GameplayLevelSetup : MonoBehaviour
    {
        private BackgroundAudioService _backgroundAudioService;

        [Inject]
        private void Construct(BackgroundAudioService backgroundAudioService)
        {
            _backgroundAudioService = backgroundAudioService;
            _backgroundAudioService.PlayBackgroundMusicByType(BackgroundMusicType.Gameplay);
        }
    }
}
