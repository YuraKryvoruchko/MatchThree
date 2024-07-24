using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Infrastructure.Service
{
    public enum SoundType
    {
        Click,
        Switch
    }
    public class UIAudioService : IDisposable
    {
        [Header("Audio Source")]
        [SerializeField] private AudioSource _uiSource;
        [Header("Sounds")]
        [SerializeField] private AssetReferenceAudioClip _clickClip;
        [SerializeField] private AssetReferenceAudioClip _switchClip;
        [Header("Mixer Settings")]
        [SerializeField] private string _volumeParameterName;
        [SerializeField] private AudioMixerGroup _audioMixerGroup;

        private Dictionary<SoundType, AssetReferenceAudioClip> _typeClipDictionary;

        public UIAudioService(UIAudioServiceConfig config)
        {
            _uiSource = GameObject.Instantiate(config.UISourcePrefab);
            _typeClipDictionary = new Dictionary<SoundType, AssetReferenceAudioClip>()
            {
                { SoundType.Click, config.ClickClip },
                { SoundType.Switch, config.SwitchClip }
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

        public async void PlaySound(SoundType type)
        {
            AssetReferenceAudioClip clip = _typeClipDictionary[type];
            if (clip.Asset == null)
            {
                if (!clip.IsDone && clip.OperationHandle.PercentComplete < 1f)
                    await clip.OperationHandle.Task;
                else
                    await clip.LoadAssetAsync().Task;
            }
            _uiSource.PlayOneShot(clip.Asset as AudioClip);
        }
        public void SetVolume(float value)
        {
            _audioMixerGroup.audioMixer.SetFloat(_volumeParameterName, value);
        }
    }
}
