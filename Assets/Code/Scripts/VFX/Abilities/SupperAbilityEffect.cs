using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

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
        private MagicLineEffect[] _magicLines;

        public event Action<IBasicVFXEffect> OnStart;
        public event Action<IBasicVFXEffect, bool> OnPause;
        public event Action<IBasicVFXEffect> OnStoped;
        public event Action<IBasicVFXEffect> OnComplete;

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

        public async UniTask Play()
        {
            OnStart?.Invoke(this);

            Vector3[] endPositions = _parameters.EndPositions;
            Action<Vector3> OnLineReady = _parameters.OnLineReady;
            Action OnAllReady = _parameters.OnAllReady;

            _isStoped = false;
            int numberOfPositions = endPositions.Length;
            _magicLines = new MagicLineEffect[endPositions.Length];
            _particlies = new List<ParticleSystem>(endPositions.Length);

            for (int i = 0; i < endPositions.Length; i++)
            {
                if (_isPausing)
                    await UniTask.WaitWhile(() => _isPausing);
                if (_isStoped)
                    return;

                MagicLineEffect magicLine = Instantiate(_magicLineEffect, transform);
                _magicLines[i] = magicLine;
                
                float duration = Vector3.Distance(transform.position, endPositions[i]) / _lineSpeed;
                magicLine.MoveFromAndTo(transform.position, endPositions[i], duration, 
                    (line) => 
                    { 
                        _particlies.Add(Instantiate(_lightGlowPrefab, line.EndPosition, Quaternion.identity, transform));
                        numberOfPositions--;
                        OnLineReady?.Invoke(line.EndPosition);
                    });

                await UniTask.WaitForSeconds(_lineCreatingDelayInSeconds);
            }

            await UniTask.WaitWhile(() => numberOfPositions > 0 || _isStoped);
            if (_isStoped)
                return;

            OnAllReady?.Invoke();

            float particleDuretion = 0f;
            for (int i = 0; i < endPositions.Length; i++)
            {
                Destroy(_magicLines[i].gameObject);
                _magicLines[i] = null;
                ParticleSystem particle = Instantiate(_explosionPrefab, transform);
                particle.transform.position = endPositions[i];
                _particlies[i].Stop();
                _particlies[i] = particle;
                particleDuretion = particle.main.duration;
                particle.Play();
            }
            _particlies.Clear();
            await UniTask.WaitForSeconds(particleDuretion);
            OnComplete?.Invoke(this);
        }

        public void Pause(bool isPause)
        {
            if (_isPausing == isPause)
                return;

            _isPausing = isPause;
            for (int i = 0; i < _magicLines.Length; i++)
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

            OnPause?.Invoke(this, isPause);
        }
        public void Stop()
        {
            _isStoped = true;
            OnStoped?.Invoke(this);
        }
        public void SetParameters(SupperAbilityVFXParameters parameters)
        {
            _parameters = parameters;
        }
    }
}
