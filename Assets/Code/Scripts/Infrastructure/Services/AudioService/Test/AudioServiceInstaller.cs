using Core.Infrastructure.Service;
using System.Collections;
using System.Collections.Generic;
using Zenject;

public class AudioServiceInstaller : MonoInstaller
{
    public AudioServiceConfig Config;

    public override void InstallBindings()
    {
        Container
            .BindInterfacesAndSelfTo<AudioService>()
            .AsSingle()
            .WithArguments(Config);
    }
}
