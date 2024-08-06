using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Infrastructure.Service.Audio
{
    [CreateAssetMenu(fileName = "AudioServiceConfig", menuName = "SO/Audio/AudioServiceConfig")]
    public class AudioServiceConfig : ScriptableObject
    {
        public TypeGroupKey[] TypeGroups;

        [Serializable]
        public class TypeGroupKey
        {
#if UNITY_EDITOR
            [HideInInspector]
            public string EditorName;
#endif
            public AudioGroupType Type;
            public AudioMixerGroup Group;
            public string VolumeProperty;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            for(int i = 0; i < TypeGroups.Length; i++)
                TypeGroups[i].EditorName = TypeGroups[i].Type.ToString();
        }
#endif
    }
}
