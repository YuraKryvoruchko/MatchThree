using Core.Infrastructure.Service;
using UnityEngine;

namespace Assets.Code.Scripts.Infrastructure.Services.AudioService.Version
{
    [CreateAssetMenu(fileName = "ClipEvent", menuName = "SO/Audio/ClipEvent")]
    public class ClipEvent : ScriptableObject
    {
        [Header("Audio Settings")]
        public AudioGroupType AudioGroup;
        public AssetReferenceAudioClip[] Clips;
        [Header("Properties")]
        public bool IsLoop;
    }
}
