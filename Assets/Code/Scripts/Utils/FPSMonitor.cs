using UnityEngine;

namespace Core.Utils
{
    public class FPSMonitor : MonoBehaviour
    {
        private int _frameRate;

        private float _time;
        private int _frameCount;

        private const float POLLING_TIME = 1F;

        private void Update()
        {
            _time += Time.deltaTime;
            _frameCount++;

            if( _time >= POLLING_TIME)
            {
                _frameRate = Mathf.RoundToInt(_frameCount / _time);

                _time -= POLLING_TIME;
                _frameCount = 0;
            }
        }
    }
}
