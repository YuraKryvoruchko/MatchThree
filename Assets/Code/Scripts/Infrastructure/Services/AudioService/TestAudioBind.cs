using UnityEngine;
using Zenject;

namespace Core.Infrastructure.Service.Audio
{
    public class TestAudioBind : MonoInstaller
    {
        [SerializeField] private AudioServiceConfig _audioServiceConfig;

        public override void InstallBindings()
        {
            BindAudioService();
        }

        private void BindAudioService()
        {
            Container
                .Bind<AudioService>()
                .AsSingle()
                .WithArguments(_audioServiceConfig);
        }
    }
}
