using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Core.Infrastructure.Service.Audio;

namespace Core.Gameplay
{
    public class BombAbility : IAbility
    {
        private AssetReference _bombEffectReference;
        private ParticleSystem _bombEffectInstance;

        private int _lineLenght = 5;

        private GameField _gameField;

        private IAudioService _audioService;
        private ClipEvent _explosiveEvent;
        private SourceInstance _audioSourceInstance;

        public BombAbility(int lineLenght, IAudioService audioService, ClipEvent explosiveEvent, AssetReference bombEffectReference)
        {
            _lineLenght = lineLenght;
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

            int lenghtFromBombCell = (_lineLenght - 1) / 2, taskArrayIndex = 0;
            UniTask[] explodeTasks = new UniTask[_lineLenght * _lineLenght];
            for (int i = -lenghtFromBombCell; i <= lenghtFromBombCell; i++)
            {
                for (int j = -lenghtFromBombCell; j <= lenghtFromBombCell; j++, taskArrayIndex++)
                {
                    explodeTasks[taskArrayIndex] = _gameField.ExplodeCell(abilityPosition.x + i, abilityPosition.y + j);
                }
            }

            await UniTask.WhenAll(explodeTasks);
            _audioService.ReleaseSource(_audioSourceInstance);
            Addressables.ReleaseInstance(_bombEffectInstance.gameObject);
        }
    }
}
