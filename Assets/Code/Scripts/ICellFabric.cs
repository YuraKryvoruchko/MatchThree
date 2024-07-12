using UnityEngine;

public interface ICellFabric
{
    void Init();
    Cell GetCell(CellType type, Vector3 position, Quaternion rotation, Transform parent = null);
    void ReturnCell(Cell cell);
}
