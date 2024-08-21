using System;
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
        private bool _isPaused;

        private ParticleSystem[] _particlies;
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
            _particlies = new ParticleSystem[endPositions.Length * 2];

            for (int i = 0; i < endPositions.Length; i++)
            {
                if (_isPaused)
                    await UniTask.WaitWhile(() => _isPaused);
                if (_isStoped)
                    return;

                MagicLineEffect magicLine = Instantiate(_magicLineEffect, transform);
                _magicLines[i] = magicLine;
                
                float duration = Vector3.Distance(transform.position, endPositions[i]) / _lineSpeed;
                magicLine.MoveFromAndTo(transform.position, endPositions[i], duration, 
                    (line) => 
                    { 
                        numberOfPositions--;
                        _particlies[numberOfPositions] = Instantiate(_lightGlowPrefab, line.EndPosition, Quaternion.identity, transform);
                        OnLineReady?.Invoke(line.EndPosition);
                    });

                await UniTask.WaitForSeconds(_lineCreatingDelayInSeconds);
            }

            await UniTask.WaitWhile(() => numberOfPositions > 0 || _isStoped);
            if (_isStoped)
                return;

            OnAllReady?.Invoke();

            float particleDuration = 0f;
            for (int i = 0; i < endPositions.Length; i++)
            {
                Destroy(_magicLines[i].gameObject);
                _magicLines[i] = null;
                ParticleSystem particle = Instantiate(_explosionPrefab, transform);
                particle.transform.position = endPositions[i];
                _particlies[i].Stop();
                _particlies[endPositions.Length + i] = particle;
                particleDuration = particle.main.duration;
                particle.Play();
            }
            float elapsedTime = 0f;
            while(elapsedTime < particleDuration)
            {
                if(!_isPaused)
                    elapsedTime += Time.deltaTime;

                await UniTask.Yield();
            }
            OnComplete?.Invoke(this);
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
            for (int i = 0; i < _particlies.Length; i++)
            {
                if (_particlies[i] == null)
                    continue;

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
