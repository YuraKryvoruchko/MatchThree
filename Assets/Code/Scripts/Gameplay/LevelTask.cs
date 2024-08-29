using System;
using UnityEngine;

namespace Core.Gameplay
{
    [Serializable]
    public class LevelTask
    {
        public Sprite Icon;
        public int Count;
        public CellType CellType;
    }
}
