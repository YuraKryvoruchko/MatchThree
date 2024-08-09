using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Infrastructure.Service.Audio
{
    [CreateAssetMenu(fileName = "AudioServiceConfig", menuName = "SO/Audio/AudioServiceConfig")]
    public class AudioServiceConfig : ScriptableObject
    {
        [Header("Groups")]
        public TypeGroupKey[] TypeGroups;
        [Header("Snapshots")]
        public TypeSnapshotKey[] TypeSnapshots;

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
        [Serializable]
        public class TypeSnapshotKey
        {
#if UNITY_EDITOR
            [HideInInspector]
            public string EditorName;
#endif
            public AudioSnapshotType Type;
            public AudioMixerSnapshot Snapshot;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            for(int i = 0; i < TypeGroups.Length; i++)
                TypeGroups[i].EditorName = TypeGroups[i].Type.ToString();
            for (int i = 0; i < TypeSnapshots.Length; i++)
                TypeSnapshots[i].EditorName = TypeSnapshots[i].Type.ToString();
        }
#endif
    }
}
