using System;
using System.Collections.Generic;
using UnityEngine;

public class FieldCellPool : MonoBehaviour, ICellFabric
{
    [SerializeField] private CellNumberPair[] _cellNumberPairs;

    private Dictionary<CellType, Queue<Cell>> _disabledCells;

    [Serializable]
    public struct CellNumberPair
    {
        public CellType Type;
        public Cell CellPrefab;
        public int Count;
    }

    public void Awake()
    {
        _disabledCells = new Dictionary<CellType, Queue<Cell>>();
    }

    void ICellFabric.Init()
    {
        foreach(CellNumberPair pair in _cellNumberPairs)
        {
            _disabledCells.Add(pair.Type, new Queue<Cell>());
            for (int i = 0; i < pair.Count; i++)
            {
                Cell cell = Instantiate(pair.CellPrefab, new Vector3(0, 100, 0), Quaternion.identity);
                cell.gameObject.SetActive(false);
                _disabledCells[pair.Type].Enqueue(cell);
            }
        }
    }
    Cell ICellFabric.GetCell(CellType type, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        Cell cell = _disabledCells[type].Dequeue();
        cell.transform.parent = parent;
        cell.transform.localPosition = position;
        cell.transform.localRotation = rotation;
        cell.gameObject.SetActive(true);
        return cell;
    }
    void ICellFabric.ReturnCell(Cell cell)
    {
        cell.gameObject.SetActive(false);
        _disabledCells[cell.Type].Enqueue(cell);
    }
}