using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Core.Gameplay;
using Core.Infrastructure.Service.Audio;

namespace Core.Infrastructure.Factories
{
    public class AbilityFactory : IAbilityFactory, IDisposable
    {
        private Dictionary<CellType, IAbility> _cellAbilityDictionary;
        private Dictionary<CellTypeCombination, IAbility> _advencedAbilityDictionary;

        private IAudioService _audioService;

        [Serializable]
        public class AbilityFactoryConfig
        {
            [Header("Lighting Bolt Settings")]
            public AssetReferenceGameObject LightingBoltEffectPrefabReference;
            public ClipEvent LightingBoltHitEvent;
            [Header("Bomb Settings")]
            public AssetReferenceGameObject SmallBombEffectPrefabReference;
            public AssetReferenceGameObject BigBombEffectPrefabReference;
            public ClipEvent ExplosiveEvent;
            [Header("Supper Ability Settings")]
            public AssetReferenceGameObject SupperCellEffectPrefabReference;
            public ClipEvent ElementCapturingEvent;
        }

        public struct CellTypeCombination
        {
            public CellType FirstType;
            public CellType SecondType;

            public CellTypeCombination(CellType firstType, CellType secondType)
            {
                if(firstType < secondType)
                {
                    FirstType = firstType;
                    SecondType = secondType;
                }
                else
                {
                    FirstType = secondType;
                    SecondType = firstType;
                }
            }

            public override string ToString()
            {
                return $"DoubleCellType: {{{FirstType} : {SecondType}}}";
            }
        }

        public AbilityFactory(AbilityFactoryConfig config, IAudioService audioService) 
        {
            _audioService = audioService;
            _cellAbilityDictionary = new Dictionary<CellType, IAbility>() 
            {
                { CellType.Bomb, new BombAbility(3, _audioService, config.ExplosiveEvent, config.SmallBombEffectPrefabReference) },
                { CellType.LightningBolt, new LightningBoltAbility(_audioService, config.LightingBoltHitEvent, config.LightingBoltEffectPrefabReference, 3) },
                { CellType.Supper, new SupperAbility(_audioService, config.ElementCapturingEvent, config.SupperCellEffectPrefabReference) }
            };
            _advencedAbilityDictionary = new Dictionary<CellTypeCombination, IAbility>()
            {
                { new CellTypeCombination(CellType.Bomb, CellType.Bomb)
                    , new BombAbility(5, _audioService, config.ExplosiveEvent, config.BigBombEffectPrefabReference) },

                { new CellTypeCombination(CellType.Bomb, CellType.LightningBolt)
                    , new LightningBoltAbility(_audioService, config.LightingBoltHitEvent, config.LightingBoltEffectPrefabReference, 3
                    , _cellAbilityDictionary[CellType.Bomb]) },

                { new CellTypeCombination(CellType.Bomb, CellType.Supper)
                    , new ReplaycableSupperAbility(_audioService, config.ElementCapturingEvent, config.SupperCellEffectPrefabReference, CellType.Bomb
                    , _cellAbilityDictionary[CellType.Bomb], 5) },

                { new CellTypeCombination(CellType.LightningBolt, CellType.LightningBolt)
                    , new LightningBoltAbility(_audioService, config.LightingBoltHitEvent, config.LightingBoltEffectPrefabReference, 10) },

                { new CellTypeCombination(CellType.LightningBolt, CellType.Supper)
                    , new QuicklySupperAbility(_audioService, config.ElementCapturingEvent, config.SupperCellEffectPrefabReference
                    , new LightningBoltAbility(_audioService, config.LightingBoltHitEvent, config.LightingBoltEffectPrefabReference, 1), 15) },

                { new CellTypeCombination(CellType.Supper, CellType.Supper)
                    , new SupperAbility(_audioService, config.ElementCapturingEvent, config.SupperCellEffectPrefabReference) }
            };
        }
        void IDisposable.Dispose()
        {
            foreach(var keyValuePair in _cellAbilityDictionary)
            {
                if(keyValuePair.Value is IDisposable disposable)
                    disposable.Dispose();
            }
            foreach (var keyValuePair in _advencedAbilityDictionary)
            {
                if (keyValuePair.Value is IDisposable disposable)
                    disposable.Dispose();
            }
        }

        IAbility IAbilityFactory.GetAbility(CellType type)
        {
            return _cellAbilityDictionary[type];
        }
        IAbility IAbilityFactory.GetAdvancedAbility(CellType firstType, CellType secondType)
        {
            CellTypeCombination doubleCellType = new CellTypeCombination(firstType, secondType);
            if (_advencedAbilityDictionary.TryGetValue(doubleCellType, out IAbility ability))
                return ability;
            else
                throw new Exception($"Type {doubleCellType} is not added to the dictionary!");
        }
    }
}
