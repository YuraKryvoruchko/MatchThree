using UnityEngine;
using UnityEngine.Audio;

namespace Core.Infrastructure.Service
{
    [CreateAssetMenu(fileName = "AudioServiceConfig", menuName = "SO/Audio/AudioServiceConfig")]
    public class AudioServiceConfig : ScriptableObject
    {
        [Header("Groups")]
        public AudioMixerGroup MasterGroup;
        public AudioMixerGroup MusicGroup;
        public AudioMixerGroup SoundGroup;
        [Header("Group Parameters")]
        public string MasterVolumeParameter;
        public string MusicVolumeParameter;
        public string SoundVolumeParameter;
    }   
}
