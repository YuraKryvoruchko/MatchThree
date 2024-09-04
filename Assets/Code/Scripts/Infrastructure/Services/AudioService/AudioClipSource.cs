using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Infrastructure.Service.Audio
{
    public class AudioClipSource : IDisposable
    {
        private AudioSource _source;
        private AudioSourceEventModule _audioSourceEventModule;
        private ClipEvent _clipReference;

        private int _currentClipIndex = 0;

        public bool IsPause { get; private set; }

        public event Action<AudioClipSource> OnEndPlaying;

        public AudioClipSource(AudioSource source)
        {
            _source = source;
            _audioSourceEventModule = _source.gameObject.AddComponent<AudioSourceEventModule>();
        }

        public void Init(ClipEvent clipEvent, AudioMixerGroup mixerGroup = null, float spatialBlend = 0, Vector3 position = default)
        {
            _clipReference = clipEvent;
            _source.spatialBlend = spatialBlend;
            _source.outputAudioMixerGroup = mixerGroup;
            _source.transform.position = position;
            _source.gameObject.SetActive(true);
        }
        public void Deinit()
        {
            _clipReference = null;
            _source.gameObject.SetActive(false);
        }

        public async void Play()
        {
            _currentClipIndex = 0;
            AssetReferenceAudioClip clipReference = _clipReference.Clips[_currentClipIndex].AudioClip;
            _source.clip = await clipReference.GetOrLoad();
            _audioSourceEventModule.OnEndPlay += SetNextClip;
            _source.Play();
        }
        public void Stop() 
        {
            if (_source == null)
                return;

            _source.Stop();
            _audioSourceEventModule.OnEndPlay -= SetNextClip;
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
            _audioSourceEventModule.OnEndPlay -= SetNextClip;
            if(_source != null)
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
