using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using Core.Gameplay;

namespace Core.Infrastructure.Factories
{
    public class AbilityFactory : IAbilityFactory
    {
        private Dictionary<CellType, IAbility> _cellAbilityDictionary;

        [Serializable]
        public class AbilityFactoryConfig
        {
            public AssetReference LightingBoltEffectPrefabReference;
            public AssetReference BombEffectPrefabReference;
            public AssetReference SupperCellEffectPrefabReference;
        }

        public AbilityFactory(AbilityFactoryConfig config) 
        {
            _cellAbilityDictionary = new Dictionary<CellType, IAbility>() 
            {
                { CellType.Bomb, new BombAbility(config.BombEffectPrefabReference) },
                { CellType.LightningBolt, new LightingBoltAbility(config.LightingBoltEffectPrefabReference) },
                { CellType.Supper, new SupperAbility(config.SupperCellEffectPrefabReference) }
            };
        }

        IAbility IAbilityFactory.GetAbility(CellType type)
        {
            return _cellAbilityDictionary[type];
        }
    }
}
