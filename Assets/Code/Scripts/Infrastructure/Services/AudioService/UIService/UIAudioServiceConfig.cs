using UnityEngine;
using UnityEngine.Audio;

namespace Core.Infrastructure.Service
{
    [CreateAssetMenu(fileName = "UIAudioServiceConfig", menuName = "SO/Audio/UIAudioServiceConfig")]
    public class UIAudioServiceConfig : ScriptableObject
    {
        [Header("Sounds")]
        public AssetReferenceAudioClip ClickClip;
        public AssetReferenceAudioClip SwitchClip;
        [Header("Audio Group")]
        public string VolumeParameterName;
        public AudioMixerGroup Group;
    }
}
