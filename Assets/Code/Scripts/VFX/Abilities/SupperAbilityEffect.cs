using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR
using com.cyborgAssets.inspectorButtonPro;
#endif

namespace Core.VFX.Abilities
{
    public class SupperAbilityEffect : MonoBehaviour, IVFXEffect<SupperAbilityEffect.SupperAbilityVFXParameters>
    {
        [Header("Settings")]
        [SerializeField] private float _lineSpeed;
        [SerializeField] private float _lineCreatingDelayInSeconds = 0.1f;
        [SerializeField] private ParticleSystem _explosionPrefab;
        [SerializeField] private ParticleSystem _lightGlowPrefab;
        [SerializeField] private MagicLineEffect _magicLineEffect;

        private SupperAbilityVFXParameters _parameters;

        private bool _isStoped;
        private bool _isPausing;

        private List<ParticleSystem> _particlies;
        private List<MagicLineEffect> _magicLines;

        public event Action OnEnd;

        public class SupperAbilityVFXParameters
        {
            public Vector3[] EndPositions;
            public Action<Vector3> OnLineReady;
            public Action OnAllReady;

            public SupperAbilityVFXParameters(Vector3[] endPositions, Action<Vector3> OnLineReadyCallback = null, Action OnAllReadyCallback = null)
            {
                EndPositions = endPositions;
                OnLineReady = OnLineReadyCallback;
                OnAllReady = OnAllReadyCallback;
            }
        }

#if UNITY_EDITOR
        //[ProPlayButton]
        //private void Test(Vector3 point1, Vector3 point2)
        //{
        //    Play(new Vector3[] { point1, point2 }, (pos) => Debug.Log($"On Ready line and pos: {pos}"), () => Debug.Log("OnReady")).Forget();
        //}
#endif
        public async UniTask Play()
        {
            Vector3[] endPositions = _parameters.EndPositions;
            Action<Vector3> OnLineReady = _parameters.OnLineReady;
            Action OnAllReady = _parameters.OnAllReady;

            _isStoped = false;
            int numberOfPositions = endPositions.Length;
            _magicLines = new List<MagicLineEffect>(endPositions.Length);
            _particlies = new List<ParticleSystem>(endPositions.Length);

            for (int i = 0; i < endPositions.Length; i++)
            {
                if (_isPausing)
                    await UniTask.WaitWhile(() => _isPausing);
                if (_isStoped)
                    return;

                MagicLineEffect magicLine = Instantiate(_magicLineEffect, this.transform);
                _magicLines.Add(magicLine);
                
                float duration = Vector3.Distance(transform.position, endPositions[i]) / _lineSpeed;
                magicLine.MoveFromAndTo(transform.position, endPositions[i], duration, 
                    (line) => 
                    { 
                        _particlies.Add(Instantiate(_lightGlowPrefab, line.EndPosition, Quaternion.identity));
                        numberOfPositions--;
                        OnLineReady?.Invoke(line.EndPosition);
                    });

                await UniTask.WaitForSeconds(_lineCreatingDelayInSeconds);
            }

            await UniTask.WaitWhile(() => numberOfPositions > 0 || _isStoped);
            if (_isStoped)
                return;

            OnAllReady?.Invoke();
            _magicLines.Clear();

            for (int i = 0; i < endPositions.Length; i++)
            {
                ParticleSystem particle = Instantiate(_explosionPrefab, this.transform);
                particle.transform.position = endPositions[i];
                _particlies[i].Stop();
                _particlies[i] = particle;
                particle.Play();
            }
            _particlies.Clear();
        }

        public void Pause(bool isPause)
        {
            if (_isPausing == isPause)
                return;

            _isPausing = isPause;
            for (int i = 0; i < _magicLines.Count; i++)
            {
                _magicLines[i].SetPause(isPause);
            }
            for (int i = 0; i < _particlies.Count; i++)
            {
                if (isPause)
                    _particlies[i].Pause();
                else
                    _particlies[i].Play();
            }
        }
        public void Stop()
        {
            _isStoped = true;
        }
        public void SetParameters(SupperAbilityVFXParameters parameters)
        {
            _parameters = parameters;
        }
    }
}
