using System;
using Core.Gameplay;
using UnityEngine.AddressableAssets;

namespace Core.UI.Gameplay
{
    [Serializable]
    public class HolderAbilitySettings
    {
        public AssetReferenceSprite Icon;
        public CellType AbilityType;
        public int Amount;
    }
}
