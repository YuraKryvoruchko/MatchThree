using System;
using UnityEngine;
using Core.Gameplay;
using System.Collections.Generic;

namespace Core.Infrastructure.Factories
{
    public class FieldCellFabric : ICellFabric
    {
        private Stack<Cell> _cellStack;
        private Dictionary<CellType, CellConfig> _cellConfigs;

        private FieldCellFabricConfig _config;

        [Serializable]
        public class FieldCellFabricConfig
        {
            public Cell CellPrefab;
            public CellConfig[] CellConfigs;
            public int CellCount;
            public Transform CellContainer;
        }

        public FieldCellFabric(FieldCellFabricConfig config)
        {
            _cellStack = new Stack<Cell>(config.CellCount);
            _cellConfigs = new Dictionary<CellType, CellConfig>(config.CellConfigs.Length);
            _config = config;
        }

        void ICellFabric.Init()
        {
            foreach(CellConfig cellConfig in _config.CellConfigs)
            {
                _cellConfigs.Add(cellConfig.Type, cellConfig);
            }
            for(int i = 0; i < _config.CellCount; i++)
            {
                Cell cell = GameObject.Instantiate(_config.CellPrefab, _config.CellContainer);
                cell.gameObject.SetActive(false);
                _cellStack.Push(cell);
            }
        }
        Cell ICellFabric.GetCell(CellType type, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            Cell cell = _cellStack.Pop();
            cell.transform.parent = parent;
            cell.transform.localPosition = position;
            cell.transform.localRotation = rotation;
            cell.transform.localScale = Vector3.one;
            cell.gameObject.SetActive(true);
            cell.Init(_cellConfigs[type]);
            return cell;
        }
        void ICellFabric.ReturnCell(Cell cell)
        {
            cell.gameObject.SetActive(false);
            cell.transform.parent = _config.CellContainer;
            _cellStack.Push(cell);
        }
    }
}