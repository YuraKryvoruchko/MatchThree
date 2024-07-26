using Core.Infrastructure.Service;
using UnityEngine;
using Zenject;

public class TestAudioService : MonoBehaviour
{
    public AudioFile AudioFile;
    public string _key;

    [Inject]
    private void Construct(AudioService audioService)
    {
        audioService.LoadAudioFile(AudioFile);
        audioService.Play(AudioFile.Type, _key);
    }
}
