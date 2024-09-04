using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Infrastructure.Service.Audio
{
    public class AudioClipSourcePool : IDisposable
    {
        private Queue<AudioClipSource> _audioClipSources;

        private Transform _audioSourceContainer;

        private int _startCapacity;
        private int _numberSource = 0;

        public AudioClipSourcePool(Transform audioSourceContainer, int startCapacity = 15)
        {
            _startCapacity = startCapacity;
            _audioClipSources = new Queue<AudioClipSource>(startCapacity);
            _audioSourceContainer = audioSourceContainer;
        }
        public void Dispose()
        {
            foreach(AudioClipSource audioClipSource in _audioClipSources)
            {
                audioClipSource.Dispose();
            }
        }

        public void Init()
        {
            CreateNewAudioClipSource();
        }

        public AudioClipSource GetAudioClipSource(ClipEvent clipEvent, AudioMixerGroup mixerGroup = null, 
            float spatialBlend = 0, Vector3 position = default)
        {
            if (_audioClipSources.Count == 0)
                CreateNewAudioClipSource();

            AudioClipSource source = _audioClipSources.Dequeue();
            source.Init(clipEvent, mixerGroup, spatialBlend, position);
            return source;
        }
        public void ReturnAudioClipSource(AudioClipSource audioClipSource)
        {
            audioClipSource.Deinit();
            _audioClipSources.Enqueue(audioClipSource);
        }

        private void CreateNewAudioClipSource()
        {
            for (int i = 0; i < _startCapacity; i++, _numberSource++)
            {
                AudioSource audioSource = new GameObject("AudioSource " + _numberSource).AddComponent<AudioSource>();
                audioSource.transform.parent = _audioSourceContainer;
                audioSource.playOnAwake = false;
                audioSource.gameObject.SetActive(false);
                _audioClipSources.Enqueue(new AudioClipSource(audioSource));
            }
        }
    }
}
