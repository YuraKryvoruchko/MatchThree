using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using com.cyborgAssets.inspectorButtonPro;
using Core.Extensions;

namespace Core.VFX.Abilities
{
    public class SupperAbilityEffect : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _lineSpeed;

        [SerializeField] private AssetReference _explosionPrefabReference;
        [SerializeField] private LineRenderer _linePrefab;
        [Header("Components")]
        [SerializeField] private AudioSource _audioSource;

        private List<ParticleSystem> _particles;

        [ProPlayButton]
        private void Test(Vector3 point1, Vector3 point2)
        {
            Play(new Vector3[] { point1, point2 }, () => Debug.Log("end"));
        }

        public async void Play(Vector3[] endPositions, Action OnReadyLine = null)
        {
            _audioSource.Play();
            UniTask[] moveTasks = new UniTask[endPositions.Length];
            for(int i = 0; i < endPositions.Length; i++)
            {
                LineRenderer line = Instantiate(_linePrefab, this.transform);
                line.SetPosition(0, transform.position);
                moveTasks[i] = line.MoveToAsync(1, endPositions[i], _lineSpeed);
            }

            await moveTasks;
            OnReadyLine?.Invoke();

            _particles = new List<ParticleSystem>(endPositions.Length);
            for (int i = 0; i < endPositions.Length; i++)
            {
                ParticleSystem particle = (await Addressables.InstantiateAsync(_explosionPrefabReference, this.transform))
                    .GetComponent<ParticleSystem>();
                particle.Play();
                _particles.Add(particle);
            }
            _particles.Clear();
        }

        public void Pause(bool pause)
        {

        }
    }
}
