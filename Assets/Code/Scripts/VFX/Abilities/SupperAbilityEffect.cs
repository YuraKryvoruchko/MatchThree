using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

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
        [SerializeField] private MagicLineEffect _magicLineEffect;

        private bool _isPausing;

        private List<ParticleSystem> _particlies;
        private List<MagicLineEffect> _magicLines;

#if UNITY_EDITOR
        [ProPlayButton]
        private void Test(Vector3 point1, Vector3 point2)
        {
            Play(new Vector3[] { point1, point2 }, (pos) => Debug.Log($"On Ready line and pos: {pos}"), () => Debug.Log("OnReady")).Forget();
        }
#endif
        public async UniTask Play(Vector3[] endPositions, Action<Vector3> OnLineReady = null, Action OnAllReady = null)
        {
            _magicLines = new List<MagicLineEffect>(endPositions.Length);
            _particlies = new List<ParticleSystem>(endPositions.Length);
            int numberOfPositions = endPositions.Length;
            for (int i = 0; i < endPositions.Length; i++)
            {
                if (_isPausing)
                    await UniTask.WaitWhile(() => _isPausing);

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

            await UniTask.WaitWhile(() => numberOfPositions > 0);
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

        public void Pause(bool pause)
        {
            if (_isPausing == pause)
                return;

            _isPausing = pause;
            for (int i = 0; i < _magicLines.Count; i++)
            {
                _magicLines[i].SetPause(pause);
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
