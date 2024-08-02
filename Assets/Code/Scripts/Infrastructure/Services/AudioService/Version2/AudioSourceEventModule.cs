using System;
using UnityEngine;

namespace Assets.Code.Scripts.Infrastructure.Services.AudioService.Version
{
    public class AudioSourceEventModule : MonoBehaviour
    {
        private AudioSource _source;

        public event Action OnEndPlay;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
        }
        private void Update()
        {
            if (_source.time >= _source.clip.length)
            {
                OnEndPlay?.Invoke();
            }
        }
    }
}
