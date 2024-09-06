using System;
using TMPro;
using UnityEngine;

namespace Core.Utils
{
    public class FPSMonitor : MonoBehaviour
    {
        [SerializeField] private TMP_Text _fpsText;
        [SerializeField] private TMP_Text _trargetFrameRateText;

        private string _fpsTextDescription;
        private string _targetFrameRateTextDescription;

        private int _frameRate;

        private float _time;
        private int _frameCount;

        private const float POLLING_TIME = 1F;

        private void Start()
        {
            _fpsTextDescription = _fpsText.text;
            _targetFrameRateTextDescription = _trargetFrameRateText.text;
        }
        private void Update()
        {
            UpdateTargetFrameRate();
            UpdateCurrentFrameRate();
        }

        private void UpdateTargetFrameRate()
        {
            _trargetFrameRateText.text = _targetFrameRateTextDescription + Math.Round(Screen.currentResolution.refreshRateRatio.value).ToString();
        }
        private void UpdateCurrentFrameRate()
        {
            _time += Time.deltaTime;
            _frameCount++;

            if (_time >= POLLING_TIME)
            {
                _frameRate = Mathf.RoundToInt(_frameCount / _time);
                _fpsText.text = _fpsTextDescription + _frameRate.ToString();

                _time -= POLLING_TIME;
                _frameCount = 0;
            }
        }
    }
}
