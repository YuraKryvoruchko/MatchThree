using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Core.Infrastructure.Service.Audio;
using Core.VFX;

namespace Core.Gameplay
{
    public class BombAbility : IAbility
    {
        private AssetReference _bombEffectReference;

        private int _lineLenght = 5;

        private GameField _gameField;

        private IAudioService _audioService;
        private ClipEvent _explosiveEvent;

        private event Action<bool> OnPause;

        public BombAbility(int lineLenght, IAudioService audioService, ClipEvent explosiveEvent, AssetReference bombEffectReference)
        {
            _lineLenght = lineLenght;
            _audioService = audioService;
            _explosiveEvent = explosiveEvent;
            _bombEffectReference = bombEffectReference;
        }

        public void Init(GameField gameField)
        {
            if (_gameField != null)
                _gameField.OnPause -= SetPause;

            _gameField = gameField;
            _gameField.OnPause += SetPause;
        }
        public void SetPause(bool isPause)
        {
            OnPause?.Invoke(isPause);
        }
        public async UniTask Execute(Vector2Int swipedCellPosition, Vector2Int abilityPosition)
        {
            Vector3 cellPosition = _gameField.CellPositionToWorld(abilityPosition);
            AudioClipSource audioSourceInstance = _audioService.PlayWithSource(_explosiveEvent);
            IBasicVFXEffect bombVFXEffect = (await Addressables.InstantiateAsync(_bombEffectReference,
                cellPosition, Quaternion.identity)).GetComponent<IBasicVFXEffect>();

            OnPause += bombVFXEffect.Pause;
            OnPause += audioSourceInstance.Pause;

            UniTask explosiveVFXAnimationTask = bombVFXEffect.Play();

            int lenghtFromBombCell = (_lineLenght - 1) / 2, taskArrayIndex = 0;
            UniTask[] explodeTasks = new UniTask[_lineLenght * _lineLenght + 1];
            for (int i = -lenghtFromBombCell; i <= lenghtFromBombCell; i++)
            {
                for (int j = -lenghtFromBombCell; j <= lenghtFromBombCell; j++, taskArrayIndex++)
                {
                    explodeTasks[taskArrayIndex] = _gameField.ExplodeCellAsync(new Vector2Int(abilityPosition.x + i, abilityPosition.y + j));
                }
            }
            explodeTasks[^1] = explosiveVFXAnimationTask;
            await UniTask.WhenAll(explodeTasks);

            OnPause -= bombVFXEffect.Pause;
            OnPause -= audioSourceInstance.Pause;
            _audioService.ReleaseSource(audioSourceInstance);
            Addressables.ReleaseInstance((bombVFXEffect as MonoBehaviour).gameObject);
        }
    }
}
