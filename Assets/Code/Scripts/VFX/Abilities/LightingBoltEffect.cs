using UnityEngine;
using UnityEngine.UI;
using DigitalRuby.LightningBolt;
using DG.Tweening;
using com.cyborgAssets.inspectorButtonPro;

namespace Core.VFX.Abilities
{
    public class LightingBoltEffect : MonoBehaviour
    {
        [SerializeField] private LightningBoltScript _lightningBolt;
        [SerializeField] private Image _backgroundImage;

        private Tween _colorTween;

        private bool _isPaused;

        [ProPlayButton]
        public void Play(Vector3 startPosition, Vector3 endPosition)
        {
            _lightningBolt.Init();
            _lightningBolt.StartPosition = startPosition;
            _lightningBolt.EndPosition = endPosition;
            _lightningBolt.Trigger();
            _colorTween = _backgroundImage.DOColor(new Color(0f, 0f, 0f, 0.52f), 0.5f).SetEase(Ease.OutElastic).OnComplete(() => 
            {
                _colorTween = _backgroundImage.DOColor(new Color(0f, 0f, 0f, 0f), 0.5f);
            });
        }

        [ProPlayButton]
        public void Pause(bool pause)
        {
            _isPaused = pause;
            _lightningBolt.SetPause(pause);
            if (pause && _colorTween.IsPlaying())
                _colorTween.Pause();
            else
                _colorTween.Play();
        }
    }
}
