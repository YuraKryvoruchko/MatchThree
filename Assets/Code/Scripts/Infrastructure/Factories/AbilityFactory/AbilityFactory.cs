using System;
using System.Collections.Generic;
using UnityEngine;
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
            public LightningBoltAbilityConfig SingleLightningBoltAbilityConfig;
            public LightningBoltAbilityConfig TripleLightningBoltAbilityConfig;
            public LightningBoltAbilityConfig DecimalLightningBoltAbilityConfig;
            [Header("Bomb Settings")]
            public BombAbilityConfig SmallBombAbilityConfig;
            public BombAbilityConfig BigBombAbilityConfig;
            [Header("Supper Ability Settings")]
            public BaseSupperAbilityConfig BaseSupperAbilityConfig;
            public ReplaceableSupperAbilityConfig ReplaceableSupperAbilityConfig;
            public QuicklySupperAbilityConfig QuicklySupperAbilityConfig;
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
                { CellType.Bomb, new BombAbility(_audioService, config.SmallBombAbilityConfig) },
                { CellType.LightningBolt, new LightningBoltAbility(_audioService, config.TripleLightningBoltAbilityConfig) },
                { CellType.Supper, new SupperAbility(_audioService, config.BaseSupperAbilityConfig) }
            };
            _advencedAbilityDictionary = new Dictionary<CellTypeCombination, IAbility>()
            {
                { new CellTypeCombination(CellType.Bomb, CellType.Bomb)
                    , new BombAbility(_audioService, config.BigBombAbilityConfig) },

                { new CellTypeCombination(CellType.Bomb, CellType.LightningBolt)
                    , new LightningBoltAbility(_audioService, config.TripleLightningBoltAbilityConfig, _cellAbilityDictionary[CellType.Bomb]) },

                { new CellTypeCombination(CellType.Bomb, CellType.Supper)
                    , new ReplaceableSupperAbility(_audioService, _cellAbilityDictionary[CellType.Bomb], config.ReplaceableSupperAbilityConfig) },

                { new CellTypeCombination(CellType.LightningBolt, CellType.LightningBolt)
                    , new LightningBoltAbility(_audioService, config.DecimalLightningBoltAbilityConfig) },

                { new CellTypeCombination(CellType.LightningBolt, CellType.Supper)
                    , new QuicklySupperAbility(_audioService, new LightningBoltAbility(_audioService, config.SingleLightningBoltAbilityConfig),
                    config.QuicklySupperAbilityConfig) },

                { new CellTypeCombination(CellType.Supper, CellType.Supper)
                    , new SupperAbility(_audioService, config.BaseSupperAbilityConfig) }
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
