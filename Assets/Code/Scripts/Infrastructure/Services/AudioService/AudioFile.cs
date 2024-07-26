using System;
using UnityEngine;

namespace Core.Infrastructure.Service
{
    [CreateAssetMenu(fileName = "AudioFile", menuName = "SO/Audio/AudioFile")]
    public class AudioFile : ScriptableObject
    {
        public AudioGroupType GroupType;
        public AudioFileType Type;
        public ClipKey[] AudioClips;

        [Serializable]
        public struct ClipKey
        {
            [Header("Default")]
            public string Key;
            public AssetReferenceAudioClip AudioClip;
            [Header("Properties")]
            public bool LoadImmediately;
        }
    }
}
