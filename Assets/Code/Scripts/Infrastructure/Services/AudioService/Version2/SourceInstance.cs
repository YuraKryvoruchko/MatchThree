using Core.Infrastructure.Service;
using System;
using UnityEngine;

namespace Assets.Code.Scripts.Infrastructure.Services.AudioService.Version
{
    public class SourceInstance : IDisposable
    {
        private AudioSource _source;
        private ClipEvent _clipReference;

        private int _currentClipIndex = 0;

        public bool IsPause { get; private set; }

        public event Action<SourceInstance> OnEndPlaying;

        public SourceInstance(AudioSource source, ClipEvent clipReference)
        {
            _source = source;
            _clipReference = clipReference;
        }

        public async void Play()
        {
            _currentClipIndex = 0;
            AssetReferenceAudioClip clipReference = _clipReference.Clips[_currentClipIndex].AudioClip;
            _source.clip = await clipReference.GetOrLoad();
            AudioSourceEventModule module = _source.gameObject.AddComponent<AudioSourceEventModule>();
            module.OnEndPlay += SetNextClip;
            _source.Play();
        }
        public void Stop() 
        {
            _source.Stop();
            if(_source.TryGetComponent(out AudioSourceEventModule module))
            {
                module.OnEndPlay -= SetNextClip;
                Component.Destroy(module);
            }
        }
        public void Pause(bool isPause)
        {
            IsPause = isPause;
            if (isPause)
                _source.Pause();
            else
                _source.UnPause();
        }

        public void Dispose()
        {
            GameObject.Destroy(_source.gameObject);
        }

        private async void SetNextClip()
        {
            _currentClipIndex++;
            if(_clipReference.Clips.Length == _currentClipIndex)
            {
                if (_clipReference.IsLoop)
                {
                    _currentClipIndex = 0;
                }
                else
                {
                    _source.GetComponent<AudioSourceEventModule>().OnEndPlay -= SetNextClip;
                    OnEndPlaying?.Invoke(this);
                    return;
                }
            }

            AssetReferenceAudioClip clipReference = _clipReference.Clips[_currentClipIndex].AudioClip;
            _source.clip = await clipReference.GetOrLoad();
            _source.Play();
        }
    }
}
