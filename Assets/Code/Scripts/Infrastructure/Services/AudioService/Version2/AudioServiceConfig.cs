using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Assets.Code.Scripts.Infrastructure.Services.AudioService.Version
{
    [CreateAssetMenu(fileName = "AudioServiceConfig2", menuName = "SO/Audio/AudioServiceConfig2")]
    public class AudioServiceConfig : ScriptableObject
    {
        public TypeGroupKey[] TypeGroups;

        [Serializable]
        public class TypeGroupKey
        {
            public AudioGroupType Type;
            public AudioMixerGroup Group;
            public string VolumeProperty;
        }
    }
}
