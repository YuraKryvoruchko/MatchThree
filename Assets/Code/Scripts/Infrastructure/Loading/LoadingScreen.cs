using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;

namespace Core.Infrastructure.Loading
{
    public class LoadingScreen : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private TMP_Text _descriptionText;

        public async UniTask Load(Queue<ILoadingOperation> loadingOperations)
        {
            foreach (ILoadingOperation operation in loadingOperations)
            {
                _progressSlider.value = 0;

                _descriptionText.text = operation.Description;
                await operation.Load(OnUpdateProgress);
            }
        }

        private void OnUpdateProgress(float progress)
        {
            _progressSlider.value = progress;
        }
    }
}
