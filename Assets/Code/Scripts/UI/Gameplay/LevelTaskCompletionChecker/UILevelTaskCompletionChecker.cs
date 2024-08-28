using Core.Gameplay;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Core.UI.Gameplay
{
    public class UILevelTaskCompletionChecker : MonoBehaviour
    {
        [Header("Container")]
        [SerializeField] private Transform _itemContainer;
        [Header("Prefabs")]
        [SerializeField] private GameObject _plusPrefab;
        [SerializeField] private UITaskItem _itemPrefab;

        private Dictionary<CellType, UITaskItem> _taskItems;

        private LevelTaskCompletionChecker _taskChecker;

        [Inject]
        private void Construct(LevelTaskCompletionChecker taskChecker)
        {
            _taskChecker = taskChecker;
            _taskItems = new Dictionary<CellType, UITaskItem>();
        }
        private void Start()
        {
            _taskChecker.OnExplodeCell += HandleCellExplosion;
            CreateItems();
        }
        private void OnDestroy()
        {
            _taskChecker.OnExplodeCell -= HandleCellExplosion;
        }

        private void HandleCellExplosion(CellType type, int count)
        {
            _taskItems[type].UpdateCount(count);
        }
        private void CreateItems()
        {
            for(int i = 0; i < _taskChecker.Tasks.Length; i++)
            {
                UITaskItem taskItem = Instantiate(_itemPrefab, _itemContainer);
                taskItem.SetIcon(_taskChecker.Tasks[i].Icon);
                taskItem.UpdateCount(_taskChecker.Tasks[i].Count);
                _taskItems.Add(_taskChecker.Tasks[i].CellType, taskItem);

                if (i < _taskChecker.Tasks.Length - 1)
                    Instantiate(_plusPrefab, _itemContainer);
            }
        }
    }
}
