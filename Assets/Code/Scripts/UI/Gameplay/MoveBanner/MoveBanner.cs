using UnityEngine;
using Zenject;
using TMPro;
using Core.Gameplay;
using Core.Infrastructure.Service;
using Core.Infrastructure.Gameplay;

namespace Core.UI.Gameplay 
{
    public class MoveBanner : MonoBehaviour
    {
        [SerializeField] private TMP_Text _moveCountText;

        private PlayerMoveTracking _moveTracking;

        private ILevelService _levelService;

        private int _maxMoveCount;

        private const int ACCUMULATION_MODE = -1;

        [Inject]
        private void Construct(PlayerMoveTracking playerMoveTracking, ILevelService levelService)
        {
            _moveTracking = playerMoveTracking;
            _levelService = levelService;
        }

        private void Start()
        {
            if (_levelService.IsLevelConfigSeted() 
                && _levelService.GetCurrentLevelConfig().MoveCount != LevelConfig.ACCUMULATION_MODE)
            {
                _maxMoveCount = _levelService.GetCurrentLevelConfig().MoveCount;
                _moveCountText.text = _maxMoveCount.ToString();
            }
            else
            {
                _maxMoveCount = ACCUMULATION_MODE;
                _moveCountText.text = "0";
            }

            _moveTracking.OnMove += HandleMove;
        }
        private void OnDestroy()
        {
            _moveTracking.OnMove -= HandleMove;
        }

        private void HandleMove()
        {
            if(_maxMoveCount == ACCUMULATION_MODE)
                _moveCountText.text = _moveTracking.Count.ToString();
            else
                _moveCountText.text = (_maxMoveCount - _moveTracking.Count).ToString();
        }
    }
}
