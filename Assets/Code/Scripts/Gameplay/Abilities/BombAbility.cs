using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Core.Infrastructure.Service.Audio;
using System.Collections.Generic;

namespace Core.Gameplay
{
    public class BombAbility : IAbility
    {
        private AssetReference _bombEffectReference;
        private ParticleSystem _bombEffectInstance;

        private int _horizontalLineCount;
        private int _verticalLineCount;
        private int _lineLenght;

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
        async UniTask IAbility.Execute(Vector2Int swipedCellPosition, Vector2Int abilityPosition)
        {
            Cell bombCell = _gameField.GetCell(abilityPosition.x, abilityPosition.y);
            _bombEffectInstance = (await Addressables.InstantiateAsync(_bombEffectReference, 
                bombCell.transform.position, Quaternion.identity)).GetComponent<ParticleSystem>();
            _bombEffectInstance.Play();
            _audioSourceInstance = _audioService.PlayWithSource(_explosiveEvent);
            await UniTask.WhenAll(
                _gameField.ExplodeCell(abilityPosition.x, abilityPosition.y),
                _gameField.ExplodeCell(abilityPosition.x + 1, abilityPosition.y),
                _gameField.ExplodeCell(abilityPosition.x - 1, abilityPosition.y),
                _gameField.ExplodeCell(abilityPosition.x, abilityPosition.y + 1),
                _gameField.ExplodeCell(abilityPosition.x, abilityPosition.y - 1),
                _gameField.ExplodeCell(abilityPosition.x + 1, abilityPosition.y + 1),
                _gameField.ExplodeCell(abilityPosition.x + 1, abilityPosition.y - 1),
                _gameField.ExplodeCell(abilityPosition.x - 1, abilityPosition.y + 1),
                _gameField.ExplodeCell(abilityPosition.x - 1, abilityPosition.y - 1)
            );
            _audioService.ReleaseSource(_audioSourceInstance);
            Addressables.ReleaseInstance(_bombEffectInstance.gameObject);
        }
    }
}
