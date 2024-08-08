using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using Core.Gameplay;
using Core.Infrastructure.Service.Audio;
using UnityEngine;

namespace Core.Infrastructure.Factories
{
    public class AbilityFactory : IAbilityFactory
    {
        private Dictionary<CellType, IAbility> _cellAbilityDictionary;

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

        public AbilityFactory(AbilityFactoryConfig config, IAudioService audioService) 
        {
            _audioService = audioService;
            _cellAbilityDictionary = new Dictionary<CellType, IAbility>() 
            {
                { CellType.Bomb, new BombAbility(_audioService, config.ExplosiveEvent, config.BombEffectPrefabReference) },
                { CellType.LightningBolt, new LightingBoltAbility(_audioService, config.LightingBoltHitEvent, config.LightingBoltEffectPrefabReference) },
                { CellType.Supper, new SupperAbility(_audioService, config.ElementCapturingEvent, config.SupperCellEffectPrefabReference) }
            };
        }

        IAbility IAbilityFactory.GetAbility(CellType type)
        {
            return _cellAbilityDictionary[type];
        }
    }
}
