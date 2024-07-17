using UnityEngine;
using DigitalRuby.LightningBolt;

namespace Core.VFX.Abilities
{
    public class LightingEffect : MonoBehaviour
    {
        [SerializeField] private LightningBoltScript _lightningBoltPrefab;
        [SerializeField] private Sprite _asda;

        private bool _isPaused;

        public void Play(Vector3 startPosition, Vector3 endPosition)
        {
            LightningBoltScript lightningBolt = _lightningBoltPrefab;//Instantiate(_lightningBoltPrefab, this.transform);
            lightningBolt.Init();
            lightningBolt.StartPosition = startPosition;
            lightningBolt.EndPosition = endPosition;
            lightningBolt.Trigger();
        }
        public void Pause(bool pause)
        {
            
        }
    }
}
