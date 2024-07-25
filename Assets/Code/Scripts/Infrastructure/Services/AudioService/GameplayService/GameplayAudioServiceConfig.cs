using UnityEngine;
using UnityEngine.Audio;

namespace Core.Infrastructure.Service
{
    [CreateAssetMenu(fileName = "GameplayAudioServiceConfig", menuName = "SO/Audio/GameplayAudioServiceConfig")]
    public class GameplayAudioServiceConfig : ScriptableObject
    {
        [Header("Sounds")]
        public AssetReferenceAudioClip SwitchClip;
        public AssetReferenceAudioClip DestroyClip;
        [Header("Audio Group")]
        public string VolumeParameterName;
        public AudioMixerGroup Group;
    }
}
