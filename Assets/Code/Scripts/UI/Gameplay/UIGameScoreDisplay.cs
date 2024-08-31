using TMPro;
using UnityEngine;
using Zenject;
using Core.Gameplay;

namespace Core.UI.Gameplay
{
    public class UIGameScoreDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreText;

        private GameScoreTracking _gameScoreTracking;

        [Inject]
        private void Construct(GameScoreTracking gameScoreTracking)
        {
            _gameScoreTracking = gameScoreTracking;
            _gameScoreTracking.OnUpdate += UpdateText;
        }

        private void OnDestroy()
        {
            _gameScoreTracking.OnUpdate -= UpdateText;
        }

        private void UpdateText()
        {
            _scoreText.text = _gameScoreTracking.CurrentScore.ToString();
        }
    }
}
