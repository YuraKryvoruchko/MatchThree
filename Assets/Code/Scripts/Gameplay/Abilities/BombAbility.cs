using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Core.Infrastructure.Service.Audio;
using Unity.VisualScripting;

namespace Core.Gameplay
{
    public class BombAbility : IAbility
    {
        private AssetReference _bombEffectReference;
        private ParticleSystem _bombEffectInstance;

        private GameField _gameField;

        private IAudioService _audioService;
        private ClipEvent _explosiveEvent;
        private SourceInstance _audioSourceInstance;

        public BombAbility(IAudioService audioService, ClipEvent explosiveEvent, AssetReference bombEffectReference)
        {
            _audioService = audioService;
            _explosiveEvent = explosiveEvent;
            _bombEffectReference = bombEffectReference;
        }

        void IAbility.Init(GameField gameField)
        {
            _gameField = gameField;
        }
        void IAbility.SetPause(bool isPause)
        {
            if (isPause)
                _bombEffectInstance.Pause();
            else
                _bombEffectInstance.Play();

            _audioSourceInstance.Pause(isPause);
        }
        async UniTask IAbility.Execute(int xPosition, int yPosition)
        {
            Cell bombCell = _gameField.GetCell(xPosition, yPosition);
            _bombEffectInstance = (await Addressables.InstantiateAsync(_bombEffectReference, 
                bombCell.transform.position, Quaternion.identity)).GetComponent<ParticleSystem>();
            _bombEffectInstance.Play();
            _audioSourceInstance = _audioService.PlayWithSource(_explosiveEvent);
            await UniTask.WhenAll(
                _gameField.ExplodeCell(xPosition, yPosition),
                _gameField.ExplodeCell(xPosition + 1, yPosition),
                _gameField.ExplodeCell(xPosition - 1, yPosition),
                _gameField.ExplodeCell(xPosition, yPosition + 1),
                _gameField.ExplodeCell(xPosition, yPosition - 1),
                _gameField.ExplodeCell(xPosition + 1, yPosition + 1),
                _gameField.ExplodeCell(xPosition + 1, yPosition - 1),
                _gameField.ExplodeCell(xPosition - 1, yPosition + 1),
                _gameField.ExplodeCell(xPosition - 1, yPosition - 1)
            );
            _audioService.ReleaseSource(_audioSourceInstance);
            Addressables.ReleaseInstance(_bombEffectInstance.gameObject);
        }
    }
}
