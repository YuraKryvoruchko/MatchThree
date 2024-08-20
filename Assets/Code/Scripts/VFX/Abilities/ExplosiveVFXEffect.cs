using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

namespace Core.VFX
{
    public class ExplosiveVFXEffect : MonoBehaviour, IVFXEffect<ExplosiveVFXEffect.ExplosiveVFXEffectParameters>
    {
        [SerializeField] private ParticleSystem _explosiveEffectInstance;

        public event Action<IBasicVFXEffect> OnStart;
        public event Action<IBasicVFXEffect, bool> OnPause;
        public event Action<IBasicVFXEffect> OnComplete;
        public event Action<IBasicVFXEffect> OnStoped;

        public class ExplosiveVFXEffectParameters
        {
        }

        public void Pause(bool isPause)
        {
            if (isPause)
                _explosiveEffectInstance.Pause();
            else
                _explosiveEffectInstance.Play();

            OnPause?.Invoke(this, isPause);
        }

        public async UniTask Play()
        {
            OnStart?.Invoke(this);

            _explosiveEffectInstance.Play();
            await UniTask.WaitWhile(() => _explosiveEffectInstance.time < _explosiveEffectInstance.main.duration || !_explosiveEffectInstance.isStopped);

            if(_explosiveEffectInstance.isStopped)
                OnStoped?.Invoke(this);
            else
                OnComplete?.Invoke(this);
        }

        public void SetParameters(ExplosiveVFXEffectParameters parameters = null)
        {
        }

        public void Stop()
        {
            _explosiveEffectInstance.Stop();
        }
    }
}
