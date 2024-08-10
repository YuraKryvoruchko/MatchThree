﻿using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Core.VFX.Abilities;
using Core.Infrastructure.Service.Audio;

namespace Core.Gameplay
{
    public class LightningBoltAbility : IAbility
    {
        private GameField _gameField;
        private AssetReference _lightingBoltEffectPrefab;
        private LightingBoltEffect _lightingBoltEffect;

        private int _lightningBoltCount;

        private IAudioService _audioService;
        private ClipEvent _clipEvent;
        private SourceInstance _audioSourceInstance;

        public LightningBoltAbility(IAudioService audioService, ClipEvent hitClipEvent, AssetReference lightingBoltEffectPrefab, 
            int lightningBoltCount)
        {
            _audioService = audioService;
            _clipEvent = hitClipEvent;
            _lightingBoltEffectPrefab = lightingBoltEffectPrefab;
            _lightningBoltCount = lightningBoltCount;
        }

        void IAbility.Init(GameField gameField)
        {
            _gameField = gameField;
        }
        void IAbility.SetPause(bool isPause)
        {
            _lightingBoltEffect.Pause(isPause);
            _audioSourceInstance.Pause(isPause);
        }
        async UniTask IAbility.Execute(Vector2Int swipedCellPosition, Vector2Int abilityPosition)
        {
            if(_audioSourceInstance == null)
                _audioSourceInstance = _audioService.PlayWithSource(_clipEvent);

            for (int i = 0; i < _lightningBoltCount; i++)
            {
                _lightingBoltEffect = (await Addressables.InstantiateAsync(_lightingBoltEffectPrefab))
                    .GetComponent<LightingBoltEffect>();

                Cell cell = _gameField.GetCell(abilityPosition.x, abilityPosition.y);
                Vector3 startPosition = cell.transform.position;
                startPosition.y = 5;

                _lightingBoltEffect.Play(startPosition, cell.transform.position);
                await _gameField.ExplodeCell(abilityPosition.x, abilityPosition.y);

                Addressables.ReleaseInstance(_lightingBoltEffect.gameObject);
            }
            
            _audioService.ReleaseSource(_audioSourceInstance);
            _audioSourceInstance = null;
        }
    }
}
