using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR
#endif

namespace Core.VFX.Abilities
{
    public class TestVFXEffect : MonoBehaviour, IVFXEffect<TestVFXEffect.TestVFXParams>
    {
        [Header("Settings")]
        [SerializeField] private float _lineSpeed;
        [SerializeField] private float _lineCreatingDelayInSeconds = 0.1f;
        [SerializeField] private ParticleSystem _explosionPrefab;
        [SerializeField] private ParticleSystem _lightGlowPrefab;
        [SerializeField] private MagicLineEffect _magicLineEffect;

        private bool _isPausing;

        private List<ParticleSystem> _particlies;
        private List<MagicLineEffect> _magicLines;

        private TestVFXParams _params;

        public event Action OnEnd;

        public class TestVFXParams
        {
            public Vector3[] EndPositions;
            public Action<Vector3> OnLineReady;
            public Action OnAllReady;

            public TestVFXParams(Vector3[] endPositions, Action<Vector3> OnLineReadyCallback = null, Action OnAllReadyCallback = null)
            {
                EndPositions = endPositions;
                OnLineReady = OnLineReadyCallback;
                OnAllReady = OnAllReadyCallback;
            }
        }

        async UniTask IBasicVFXEffect.Play()
        {
            int numberOfPositions = _params.EndPositions.Length;
            _magicLines = new List<MagicLineEffect>(numberOfPositions);
            _particlies = new List<ParticleSystem>(numberOfPositions);
            for (int i = 0; i < numberOfPositions; i++)
            {
                if (_isPausing)
                    await UniTask.WaitWhile(() => _isPausing);

                MagicLineEffect magicLine = Instantiate(_magicLineEffect, this.transform);
                _magicLines.Add(magicLine);

                float duration = Vector3.Distance(transform.position, _params.EndPositions[i]) / _lineSpeed;
                magicLine.MoveFromAndTo(transform.position, _params.EndPositions[i], duration,
                    (line) =>
                    {
                        _particlies.Add(Instantiate(_lightGlowPrefab, line.EndPosition, Quaternion.identity));
                        numberOfPositions--;
                        _params.OnLineReady?.Invoke(line.EndPosition);
                    });

                await UniTask.WaitForSeconds(_lineCreatingDelayInSeconds);
            }

            await UniTask.WaitWhile(() => numberOfPositions > 0);
            _params.OnAllReady?.Invoke();
            _magicLines.Clear();

            for (int i = 0; i < numberOfPositions; i++)
            {
                ParticleSystem particle = Instantiate(_explosionPrefab, this.transform);
                particle.transform.position = _params.EndPositions[i];
                _particlies[i].Stop();
                _particlies[i] = particle;
                particle.Play();
            }
            _particlies.Clear();

            OnEnd?.Invoke();
        }
        void IBasicVFXEffect.Pause(bool isPause) 
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
        void IBasicVFXEffect.Stop()
        {
            throw new NotImplementedException();
        }
        void IVFXEffect<TestVFXParams>.SetParameters(TestVFXParams parameters)
        {
            _params = parameters;
        }
    }
}
