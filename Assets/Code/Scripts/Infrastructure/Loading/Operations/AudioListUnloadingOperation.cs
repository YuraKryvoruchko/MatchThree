using System;
using Cysharp.Threading.Tasks;
using Core.Infrastructure.Service.Audio;

namespace Core.Infrastructure.Loading
{
    public class AudioListUnloadingOperation : ILoadingOperation
    {
        private ClipEvent[] _events;

        string ILoadingOperation.Description => "Unloading audio...";

        public AudioListUnloadingOperation(ClipEvent[] events)
        {
            _events = events;
        }

        async UniTask ILoadingOperation.Load(Action<float> onProgress)
        {
            float progressStep = 1f / _events.Length;
            for (int i = 0; i < _events.Length; i++)
            {
                for (int j = 0; j < _events[i].Clips.Length; j++)
                {
                    ClipEvent.AudioClipSettings clipSettings = _events[i].Clips[j];
                    if (clipSettings.LoadMode != ClipEvent.LoadMode.None)
                        continue;

                    clipSettings.AudioClip.ReleaseAsset();
                }
                onProgress?.Invoke(progressStep * (i + 1));
            }
            await UniTask.Yield();
        }
    }
}
