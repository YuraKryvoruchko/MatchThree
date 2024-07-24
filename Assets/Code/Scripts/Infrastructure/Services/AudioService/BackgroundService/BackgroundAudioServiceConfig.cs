using UnityEngine;
using UnityEngine.Audio;

namespace Core.Infrastructure.Service
{
    [CreateAssetMenu(fileName = "BackgroundAudioServiceConfig", menuName = "SO/Audio/BackgroundAudioServiceConfig")]
    public class BackgroundAudioServiceConfig : ScriptableObject
    {
        [Header("Music Settings")]
        public BackgroundAudioService.AudioListByType[] AudioClips;
        [Header("Audio Group")]
        public string VolumeParameterName;
        public AudioMixerGroup MusicGroup;
    }
}
