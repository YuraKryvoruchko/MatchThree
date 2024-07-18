using UnityEngine;
using DigitalRuby.LightningBolt;
using com.cyborgAssets.inspectorButtonPro;

namespace Core.VFX.Abilities
{
    public class LightingBoltEffect : MonoBehaviour
    {
        [SerializeField] private LightningBoltScript _lightningBolt;
        [SerializeField] private AudioSource _audioSource;

        [ProPlayButton]
        public void Play(Vector3 startPosition, Vector3 endPosition)
        {
            _lightningBolt.Init();
            _lightningBolt.StartPosition = startPosition;
            _lightningBolt.EndPosition = endPosition;
            _lightningBolt.Trigger();
            _audioSource.Play();
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
