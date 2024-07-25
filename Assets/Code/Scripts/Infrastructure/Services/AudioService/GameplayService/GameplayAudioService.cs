using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Infrastructure.Service
{
    public enum VFXSoundType
    {
        ElementSwitch
    }
    public class GameplayAudioService : IDisposable
    {
        [Header("Audio Source")]
        [SerializeField] private AudioSource _uiSource;
        [Header("Sounds")]
        [SerializeField] private AssetReferenceAudioClip _switchClip;
        [Header("Mixer Settings")]
        [SerializeField] private string _volumeParameterName;
        [SerializeField] private AudioMixerGroup _audioMixerGroup;

        private Dictionary<VFXSoundType, AssetReferenceAudioClip> _typeClipDictionary;

        public GameplayAudioService(GameplayAudioServiceConfig config)
        {
            _uiSource = new GameObject("GameplayAudioServiceSource").AddComponent<AudioSource>();
            _typeClipDictionary = new Dictionary<VFXSoundType, AssetReferenceAudioClip>()
            {
                { VFXSoundType.ElementSwitch, config.SwitchClip }
            };
            _volumeParameterName = config.VolumeParameterName;
            _audioMixerGroup = config.Group;
            _uiSource.outputAudioMixerGroup = _audioMixerGroup;
        }
        void IDisposable.Dispose()
        {
            foreach (var pair in _typeClipDictionary)
                pair.Value.ReleaseAsset();
            _typeClipDictionary.Clear();
        }

        public async void PlaySound(VFXSoundType type)
        {
            AssetReferenceAudioClip clip = _typeClipDictionary[type];
            _uiSource.PlayOneShot(await clip.GetOrLoad());
        }
        public void SetVolume(float value)
        {
            _audioMixerGroup.audioMixer.SetFloat(_volumeParameterName, value);
        }
        public float GetVolume()
        {
            float value = 0f;
            _audioMixerGroup.audioMixer.GetFloat(_volumeParameterName, out value);
            return value;
        }
    }
}
