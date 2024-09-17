using System;
using UnityEngine.AddressableAssets;

namespace Core.Gameplay
{
    [Serializable]
    public class LevelTask
    {
        public AssetReferenceSprite Icon;
        public int Count;
        public CellType CellType;
    }
}
