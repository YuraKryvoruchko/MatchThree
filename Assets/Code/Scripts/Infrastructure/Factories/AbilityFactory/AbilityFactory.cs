using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Core.Gameplay;
using Core.Infrastructure.Service.Audio;

namespace Core.Infrastructure.Factories
{
    public class AbilityFactory : IAbilityFactory
    {
        private Dictionary<CellType, IAbility> _cellAbilityDictionary;
        private Dictionary<DoubleCellType, IAbility> _advencedAbilityDictionary;

        private IAudioService _audioService;

        [Serializable]
        public class AbilityFactoryConfig
        {
            [Header("Lighting Bolt Settings")]
            public AssetReference LightingBoltEffectPrefabReference;
            public ClipEvent LightingBoltHitEvent;
            [Header("Bomb Settigns")]
            public AssetReference BombEffectPrefabReference;
            public ClipEvent ExplosiveEvent;
            [Header("Supper Ability Settings")]
            public AssetReference SupperCellEffectPrefabReference;
            public ClipEvent ElementCapturingEvent;
        }

        public struct DoubleCellType
        {
            public CellType FirstType;
            public CellType SecondType;

            public DoubleCellType(CellType firstType, CellType secondType)
            {
                FirstType = firstType;
                SecondType = secondType;
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
                { CellType.Bomb, new BombAbility(_audioService, config.ExplosiveEvent, config.BombEffectPrefabReference) },
                { CellType.LightningBolt, new LightningBoltAbility(_audioService, config.LightingBoltHitEvent, config.LightingBoltEffectPrefabReference, 3) },
                { CellType.Supper, new SupperAbility(_audioService, config.ElementCapturingEvent, config.SupperCellEffectPrefabReference) }
            };
            _advencedAbilityDictionary = new Dictionary<DoubleCellType, IAbility>()
            {
                { new DoubleCellType(CellType.Bomb, CellType.Bomb), new BombAbility(_audioService, config.ExplosiveEvent, config.BombEffectPrefabReference) },
                { new DoubleCellType(CellType.Bomb, CellType.LightningBolt), new BombAbility(_audioService, config.ExplosiveEvent, config.BombEffectPrefabReference) },
                { new DoubleCellType(CellType.Bomb, CellType.Supper), new BombAbility(_audioService, config.ExplosiveEvent, config.BombEffectPrefabReference) },
                { new DoubleCellType(CellType.LightningBolt, CellType.LightningBolt), new BombAbility(_audioService, config.ExplosiveEvent, config.BombEffectPrefabReference) },
                { new DoubleCellType(CellType.LightningBolt, CellType.Supper), new BombAbility(_audioService, config.ExplosiveEvent, config.BombEffectPrefabReference) },
                { new DoubleCellType(CellType.Supper, CellType.Supper), new BombAbility(_audioService, config.ExplosiveEvent, config.BombEffectPrefabReference) }
            };
        }

        IAbility IAbilityFactory.GetAbility(CellType type)
        {
            return _cellAbilityDictionary[type];
        }
        IAbility IAbilityFactory.GetAdvancedAbility(CellType firstType, CellType secondType)
        {
            DoubleCellType doubleCellType = firstType < secondType ?
                new DoubleCellType(firstType, secondType) : new DoubleCellType(secondType, firstType);

            if (_advencedAbilityDictionary.TryGetValue(doubleCellType, out IAbility ability))
                return ability;
            else
                throw new Exception($"Type {doubleCellType} is not added to the dictionary!");
        }
    }
}
