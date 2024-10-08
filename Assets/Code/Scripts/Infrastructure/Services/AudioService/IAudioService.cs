﻿using UnityEngine;

namespace Core.Infrastructure.Service.Audio
{
    public interface IAudioService
    {
        void PlayOneShot(ClipEvent clipEvent);
        void PlayOneShotOnPoint(ClipEvent clipEvent, Vector3 position);

        AudioClipSource PlayWithSource(ClipEvent clipEvent, bool playOnAwake = true);
        AudioClipSource PlayWithSourceOnPoint(ClipEvent clipEvent, Vector3 position, bool playOnAwake = true);
        void ReleaseSource(AudioClipSource sourceInstance);

        void PauseAll(bool isPause);
        void PauseByGroup(AudioGroupType groupType, bool isPause);

        void ChangeSnapshot(AudioSnapshotType type, float timeToReach = 0f);

        public float GetVolume(AudioGroupType groupType);
        public void SetVolume(AudioGroupType groupType, float value);
    }
}
