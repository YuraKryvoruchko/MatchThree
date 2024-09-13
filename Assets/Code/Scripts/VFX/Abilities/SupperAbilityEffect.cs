using System;
using System.Threading;
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

        private bool _isStopped;
        private bool _isPaused;

        private ParticleSystem[] _particles;
        private MagicLineEffect[] _magicLines;

        public event Action<IBasicVFXEffect> OnStart;
        public event Action<IBasicVFXEffect, bool> OnPause;
        public event Action<IBasicVFXEffect> OnStopped;
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

        public async UniTask Play(CancellationToken cancellationToken = default)
        {
            OnStart?.Invoke(this);

            Vector3[] endPositions = _parameters.EndPositions;
            Action<Vector3> OnLineReady = _parameters.OnLineReady;
            Action OnAllReady = _parameters.OnAllReady;

            _isStopped = false;
            _magicLines = new MagicLineEffect[endPositions.Length];
            _particles = new ParticleSystem[endPositions.Length * 2];
            try
            {
                await MoveMagicalLinesAsync(endPositions, OnLineReady, cancellationToken);
                if (_isStopped)
                    return;

                OnAllReady?.Invoke();

                ReplaceMagicalLinesWithParticles(endPositions);

                float particleDuration = _explosionPrefab.main.duration;
                await WaitParticleDurationAsync(particleDuration, cancellationToken);

                OnComplete?.Invoke(this);
            }
            finally
            {
                DestroyAllEffectParts();
            }
        }

        public void Pause(bool isPause)
        {
            if (_isPaused == isPause)
                return;

            _isPaused = isPause;
            for (int i = 0; i < _magicLines.Length; i++)
            {
                if(_magicLines[i] != null)
                    _magicLines[i].SetPause(isPause);
            }
            for (int i = 0; i < _particles.Length; i++)
            {
                if (_particles[i] == null)
                    continue;

                if (isPause)
                    _particles[i].Pause();
                else
                    _particles[i].Play();
            }

            OnPause?.Invoke(this, isPause);
        }
        public void Stop()
        {
            _isStopped = true;
            OnStopped?.Invoke(this);
        }
        public void SetParameters(SupperAbilityVFXParameters parameters)
        {
            _parameters = parameters;
        }

        private async UniTask MoveMagicalLinesAsync(Vector3[] endPositions, Action<Vector3> OnLineReady, CancellationToken cancellationToken)
        {
            int numberOfPositions = endPositions.Length;
            for (int i = 0; i < endPositions.Length; i++)
            {
                if (_isPaused)
                    await UniTask.WaitWhile(() => _isPaused, PlayerLoopTiming.Update, cancellationToken);
                if (_isStopped)
                    return;

                MagicLineEffect magicLine = Instantiate(_magicLineEffect, transform);
                _magicLines[i] = magicLine;

                float duration = Vector3.Distance(transform.position, endPositions[i]) / _lineSpeed;
                magicLine.MoveFromAndTo(transform.position, endPositions[i], duration,
                    (line) =>
                    {
                        numberOfPositions--;
                        _particles[numberOfPositions] = Instantiate(_lightGlowPrefab, line.EndPosition, Quaternion.identity, transform);
                        OnLineReady?.Invoke(line.EndPosition);
                    });

                await UniTask.WaitForSeconds(_lineCreatingDelayInSeconds, false, PlayerLoopTiming.Update, cancellationToken);
            }

            await UniTask.WaitWhile(() => numberOfPositions > 0 || _isStopped, PlayerLoopTiming.Update, cancellationToken);
        }
        private void ReplaceMagicalLinesWithParticles(Vector3[] endPositions)
        {
            for (int i = 0; i < endPositions.Length; i++)
            {
                Destroy(_magicLines[i].gameObject);
                _magicLines[i] = null;
                ParticleSystem particle = Instantiate(_explosionPrefab, transform);
                particle.transform.position = endPositions[i];
                _particles[i].Stop();
                _particles[endPositions.Length + i] = particle;
                particle.Play();
            }
        }
        private async UniTask WaitParticleDurationAsync(float particleDuration, CancellationToken cancellationToken)
        {
            float elapsedTime = 0f;
            while (elapsedTime < particleDuration)
            {
                if (!_isPaused)
                    elapsedTime += Time.deltaTime;

                await UniTask.Yield(cancellationToken);
            }
        }
        private void DestroyAllEffectParts()
        {
            for (int i = 0; i < _magicLines.Length; i++)
            {
                if (_magicLines[i] == null)
                    continue;

                _magicLines[i].Stop(false);
                Destroy(_magicLines[i].gameObject);
            }
            for (int i = 0; i < _particles.Length; i++)
            {
                if (_particles[i] == null)
                    continue;

                Destroy(_particles[i]);
            }
        }
    }
}
