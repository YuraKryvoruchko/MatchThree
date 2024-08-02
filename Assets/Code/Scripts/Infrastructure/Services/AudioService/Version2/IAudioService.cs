using UnityEngine;

namespace Assets.Code.Scripts.Infrastructure.Services.AudioService.Version
{
    public interface IAudioService
    {
        void PlayOneShot(ClipEvent clipEvent);
        void PlayOneShotOnPoint(ClipEvent clipEvent, Vector3 position);

        SourceInstance PlayWithSource(ClipEvent clipEvent);
        SourceInstance PlayWithSourceOnPoint(ClipEvent clipEvent, Vector3 position);
        void ReleaseSource(SourceInstance sourceInstance);

        void PauseAll(bool isPause);
        void PauseByGroup(AudioGroupType groupType, bool isPause);

        public float GetVolume(AudioGroupType groupType);
        public void SetVolume(AudioGroupType groupType, float value);
    }
}
