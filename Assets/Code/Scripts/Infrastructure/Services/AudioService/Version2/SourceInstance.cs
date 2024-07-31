using Core.Infrastructure.Service;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Code.Scripts.Infrastructure.Services.AudioService.Version
{
    public class SourceInstance : IDisposable
    {
        private AudioSource _source;
        private ClipEvent _clipReference;

        private int _currentClipIndex = 0;

        public bool IsPause { get; private set; }

        public SourceInstance(AudioSource source, ClipEvent clipReference)
        {
            _source = source;
            _clipReference = clipReference;
        }

        public async void Play()
        {
            _currentClipIndex = 0;
            AssetReferenceAudioClip clipReference = _clipReference.Clips[_currentClipIndex];
            _source.clip = await clipReference.GetOrLoad();
            AudioSourceEventModule module = _source.AddComponent<AudioSourceEventModule>();
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
                    return;
                }
            }

            AssetReferenceAudioClip clipReference = _clipReference.Clips[_currentClipIndex];
            _source.clip = await clipReference.GetOrLoad();
            _source.Play();
        }
    }
}
