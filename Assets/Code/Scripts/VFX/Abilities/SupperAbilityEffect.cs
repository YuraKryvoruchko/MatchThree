using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using com.cyborgAssets.inspectorButtonPro;
#endif
using Core.Extensions;

namespace Core.VFX.Abilities
{
    public class SupperAbilityEffect : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _lineSpeed;
        [SerializeField] private float _lineCreatingDelayInSeconds = 0.1f;
        [SerializeField] private ParticleSystem _explosionPrefab;
        [SerializeField] private LineRenderer _linePrefab;
        [Header("Components")]
        [SerializeField] private AudioSource _audioSource;

        private List<ParticleSystem> _particles;

#if UNITY_EDITOR
        [ProPlayButton]
        private void Test(Vector3 point1, Vector3 point2)
        {
            Play(new Vector3[] { point1, point2 }, () => Debug.Log("end")).Forget();
        }
#endif
        public async UniTask Play(Vector3[] endPositions, Action OnReady = null)
        {
            _audioSource.Play();
            UniTask[] moveTasks = new UniTask[endPositions.Length];
            for(int i = 0; i < endPositions.Length; i++)
            {
                LineRenderer line = Instantiate(_linePrefab, this.transform);
                line.SetPosition(0, transform.position);
                moveTasks[i] = line.MoveToAsync(1, endPositions[i], _lineSpeed);
                await UniTask.WaitForSeconds(_lineCreatingDelayInSeconds);
            }

            await UniTask.WhenAll(moveTasks);
            OnReady?.Invoke();

            _particles = new List<ParticleSystem>(endPositions.Length);
            for (int i = 0; i < endPositions.Length; i++)
            {
                ParticleSystem particle = Instantiate(_explosionPrefab, this.transform);
                particle.transform.position = endPositions[i];
                particle.Play();
                _particles.Add(particle);
            }
            _particles.Clear();
        }

        public void Pause(bool pause)
        {

            for(int i = 0; i < _particles.Count; i++)
            {
                if (pause)
                    _particles[i].Pause();
                else
                    _particles[i].Play();
            }
        }
    }
}
