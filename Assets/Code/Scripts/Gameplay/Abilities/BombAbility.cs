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
        private SourceInstance _audioSourceInstance;

        private event Action<bool> OnPause;

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
            _audioSourceInstance.Pause(isPause);
            OnPause?.Invoke(isPause);
        }
        async UniTask IAbility.Execute(Vector2Int swipedCellPosition, Vector2Int abilityPosition)
        {
            Vector3 cellPosition = _gameField.CellPositionToWorld(abilityPosition);
            IBasicVFXEffect bombVFXEffect = (await Addressables.InstantiateAsync(_bombEffectReference,
                cellPosition, Quaternion.identity)).GetComponent<IBasicVFXEffect>();

            bombVFXEffect.OnComplete += ReleaseEffect;
            bombVFXEffect.OnStoped += ReleaseEffect;
            OnPause += bombVFXEffect.Pause;
            bombVFXEffect.Play().Forget();

            _audioSourceInstance = _audioService.PlayWithSource(_explosiveEvent);

            int lenghtFromBombCell = (_lineLenght - 1) / 2, taskArrayIndex = 0;
            UniTask[] explodeTasks = new UniTask[_lineLenght * _lineLenght];
            for (int i = -lenghtFromBombCell; i <= lenghtFromBombCell; i++)
            {
                for (int j = -lenghtFromBombCell; j <= lenghtFromBombCell; j++, taskArrayIndex++)
                {
                    explodeTasks[taskArrayIndex] = _gameField.ExplodeCell(new Vector2Int(abilityPosition.x + i, abilityPosition.y + j));
                }
            }

            await UniTask.WhenAll(explodeTasks);
            _audioService.ReleaseSource(_audioSourceInstance);
        }

        private void ReleaseEffect(IBasicVFXEffect effect)
        {
            effect.OnComplete -= ReleaseEffect;
            effect.OnStoped -= ReleaseEffect;
            OnPause -= effect.Pause;
            Addressables.ReleaseInstance((effect as MonoBehaviour).gameObject);
        }
    }
}
