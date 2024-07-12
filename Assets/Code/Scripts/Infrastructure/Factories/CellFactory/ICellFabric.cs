using UnityEngine;
using Core.Gameplay;

namespace Core.Infrastructure.Factories
{
    public interface ICellFabric
    {
        void Init();
        Cell GetCell(CellType type, Vector3 position, Quaternion rotation, Transform parent = null);
        void ReturnCell(Cell cell);
    }
}
