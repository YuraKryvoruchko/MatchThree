using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Gameplay
{
    public class UITaskItem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _text;

        public void SetIcon(Sprite icon)
        {
            _image.sprite = icon;
        }
        public void UpdateCount(int currentValue)
        {
            _text.text = currentValue.ToString();
        }
    }
}
