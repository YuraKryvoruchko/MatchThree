using Core.Infrastructure.Service;
using System;
using UnityEngine;

namespace Core.Infrastructure.Service.Audio
{
    [CreateAssetMenu(fileName = "ClipEvent", menuName = "SO/Audio/ClipEvent")]
    public class ClipEvent : ScriptableObject
    {
        [Header("Audio Settings")]
        public AudioGroupType AudioGroup;
        public AudioClipSettings[] Clips;
        [Header("Properties")]
        public bool IsLoop;

        [Serializable]
        public class AudioClipSettings
        {
#if UNITY_EDITOR
            [HideInInspector]
            public string EditorAssetName;
#endif
            public AssetReferenceAudioClip AudioClip;
            public LoadMode LoadMode;
            public UnloadMode UnloadMode;
        }

        public enum LoadMode
        {
            None,
            BeforeUse
        }
        public enum UnloadMode
        {
            None,
            AfterUse
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            for(int i = 0; i < Clips.Length; i++)
            {
                if (Clips[i].AudioClip == null || Clips[i].AudioClip.editorAsset == null)
                    continue;

                Clips[i].EditorAssetName = Clips[i].AudioClip.editorAsset.name;
            }
        }
#endif
    }
}
