using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace Core.UI.Menu
{
    public class SelectLevelButton : MonoBehaviour
    {
        [SerializeField] private Button _baseButton;
        [SerializeField] private TMP_Text _levelNumberText;
        [SerializeField] private StarSettings[] _starSettings;

        public int LevelIndex { get; private set; }

        public event Action<SelectLevelButton> OnClick;

        [Serializable]
        private class StarSettings
        {
            public Image Image;
            public float MinProgress;
        }

        private void Awake()
        {
            _baseButton.onClick.AddListener(() => OnClick?.Invoke(this));
        }
        private void OnDestroy()
        {
            _baseButton.onClick.RemoveAllListeners();
        }

        public void SetLevelIndex(int index)
        {
            LevelIndex = index;
            _levelNumberText.text = (LevelIndex + 1).ToString();
        }
        public void SetLevelProgress(float progress)
        {
            for(int i = 0; i < _starSettings.Length; i++)
            {
                if (_starSettings[i].MinProgress > progress)
                    break;

                _starSettings[i].Image.color = Color.white;
            }
        }
    }
}
