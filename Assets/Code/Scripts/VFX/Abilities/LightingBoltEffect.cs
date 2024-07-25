using UnityEngine;
using DigitalRuby.LightningBolt;
using Core.Infrastructure.Service;
using com.cyborgAssets.inspectorButtonPro;

namespace Core.VFX.Abilities
{
    public class LightingBoltEffect : MonoBehaviour
    {
        [Header("VFX")]
        [SerializeField] private LightningBoltScript _lightningBolt;
        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AssetReferenceAudioClip _assetReferenceAudioClip;

        [ProPlayButton]
        public async void Play(Vector3 startPosition, Vector3 endPosition)
        {
            _lightningBolt.Init();
            _lightningBolt.StartPosition = startPosition;
            _lightningBolt.EndPosition = endPosition;
            _lightningBolt.Trigger();

            _audioSource.clip = await _assetReferenceAudioClip.GetOrLoad();
            _audioSource.Play();
            _assetReferenceAudioClip.ReleaseAsset();
        }

        [ProPlayButton]
        public void Pause(bool pause)
        {
            _lightningBolt.SetPause(pause);
            if (pause)
                _audioSource.Pause();
            else
                _audioSource.UnPause();
        }
    }
}
