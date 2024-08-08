using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
#if UNITY_EDITOR
using com.cyborgAssets.inspectorButtonPro;
#endif

namespace Core.VFX.Abilities
{
    public class SupperAbilityEffect : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _lineSpeed;
        [SerializeField] private float _lineCreatingDelayInSeconds = 0.1f;
        [SerializeField] private ParticleSystem _explosionPrefab;
        [SerializeField] private ParticleSystem _lightGlowPrefab;
        [SerializeField] private LineRendererLerp _linePrefab;
        [Header("Components")]
        [SerializeField] private AudioSource _audioSource;

        private List<ParticleSystem> _particlies;
        private List<Tweener> _moveTweeners;

#if UNITY_EDITOR
        [ProPlayButton]
        private void Test(Vector3 point1, Vector3 point2)
        {
            Play(new Vector3[] { point1, point2 }, () => Debug.Log("OnReady")).Forget();
        }
#endif
        public async UniTask Play(Vector3[] endPositions, Action OnReady = null)
        {
            _audioSource.Play();
            _moveTweeners = new List<Tweener>(endPositions.Length);
            _particlies = new List<ParticleSystem>(endPositions.Length * 2);
            for (int i = 0; i < endPositions.Length; i++)
            {
                LineRendererLerp line = Instantiate(_linePrefab, this.transform);
                line.Init(transform.position, endPositions[i]);

                float moveDuration = Vector3.Distance(transform.position, endPositions[i]) / _lineSpeed;
                _moveTweeners.Add(DOTween.To(line.Lerp, 0f, 1f, moveDuration)
                    .OnComplete(() => _particlies.Add(Instantiate(_lightGlowPrefab, endPositions[i], Quaternion.identity, transform))));

                await UniTask.WaitForSeconds(_lineCreatingDelayInSeconds);
            }

            UniTask[] tasks = new UniTask[endPositions.Length];
            for(int i = 0; i < tasks.Length; i++)
                tasks[i] = _moveTweeners[i].AsyncWaitForCompletion().AsUniTask();

            await UniTask.WhenAll(tasks);
            OnReady?.Invoke();
            _moveTweeners.Clear();

            for (int i = 0; i < endPositions.Length; i++)
            {
                ParticleSystem particle = Instantiate(_explosionPrefab, this.transform);
                particle.transform.position = endPositions[i];
                particle.Play();
                _particlies.Add(particle);
            }
            _particlies.Clear();
        }

        public void Pause(bool pause)
        {
            for (int i = 0; i < _moveTweeners.Count; i++)
            {
                if (pause)
                    _moveTweeners[i].Pause();
                else
                    _moveTweeners[i].Play();
            }
            for (int i = 0; i < _particlies.Count; i++)
            {
                if (pause)
                    _particlies[i].Pause();
                else
                    _particlies[i].Play();
            }
        }
    }
}
