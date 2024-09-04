using System;
using UnityEngine;
using UnityEngine.Audio;
using Cysharp.Threading.Tasks;

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
            _audioSourceEventModule.enabled = false;
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

        public async UniTaskVoid Play()
        {
            _currentClipIndex = 0;
            AssetReferenceAudioClip clipReference = _clipReference.Clips[_currentClipIndex].AudioClip;
            _source.clip = await clipReference.GetOrLoad();
            _audioSourceEventModule.OnEndPlay += SetNextClipSync;
            _audioSourceEventModule.enabled = true;
            _source.Play();
        }
        public void Stop() 
        {
            if (_source == null)
                return;

            _source.Stop();
            _audioSourceEventModule.enabled = false;
            _audioSourceEventModule.OnEndPlay -= SetNextClipSync;
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
            _audioSourceEventModule.OnEndPlay -= SetNextClipSync;
            if(_source != null)
                GameObject.Destroy(_source.gameObject);
        }

        private void SetNextClipSync()
        {
            SetNextClip().Forget();
        }
        private async UniTaskVoid SetNextClip()
        {
            _clipReference.Clips[_currentClipIndex].AudioClip.ReleaseAsset();
            _currentClipIndex++;
            if(_clipReference.Clips.Length == _currentClipIndex)
            {
                if (_clipReference.IsLoop)
                {
                    _currentClipIndex = 0;
                }
                else
                {
                    _audioSourceEventModule.OnEndPlay -= SetNextClipSync;

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
