using System;
using UnityEngine;

namespace Core.Infrastructure.Service
{
    public interface IAudioService<T> where T : Enum
    {
        float Volume { get; set; }

        void Play(T type);
    }
    public class VFXAudioService : IAudioService<VFXSoundType>
    {
        public float Volume { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Play(VFXSoundType type)
        {
            AssetReferenceAudioClip clip = null;
            UnityEngine.AudioSource source = new UnityEngine.GameObject(clip.Asset.name).AddComponent<UnityEngine.AudioSource>();
        }
        public AudioFrame PlayWithSource(VFXSoundType type)
        {
            throw new NotImplementedException();
        }
    }
    public struct AudioFrame
    {
        public UnityEngine.AudioSource Source;
        public AssetReferenceAudioClip Clip;
    }
    public class AudioSourceDestroy : MonoBehaviour
    {
        private AudioSource _audioSource;

        private void Update()
        {
            if (!_audioSource.isPlaying && _audioSource.time >= _audioSource.clip.length)
                Destroy(gameObject);
        }
    }
}
