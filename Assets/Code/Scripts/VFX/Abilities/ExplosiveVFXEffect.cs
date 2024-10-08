﻿using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using System.Threading;

namespace Core.VFX
{
    public class ExplosiveVFXEffect : MonoBehaviour, IVFXEffect<ExplosiveVFXEffect.ExplosiveVFXEffectParameters>
    {
        [SerializeField] private ParticleSystem _explosiveEffectInstance;

        public event Action<IBasicVFXEffect> OnStart;
        public event Action<IBasicVFXEffect, bool> OnPause;
        public event Action<IBasicVFXEffect> OnComplete;
        public event Action<IBasicVFXEffect> OnStopped;

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

        public async UniTask Play(CancellationToken cancellationToken = default)
        {
            OnStart?.Invoke(this);

            _explosiveEffectInstance.Play();
            await UniTask.WaitWhile(() => _explosiveEffectInstance.time < _explosiveEffectInstance.main.duration 
            || !_explosiveEffectInstance.isStopped, PlayerLoopTiming.Update, cancellationToken);

            if(_explosiveEffectInstance.isStopped)
                OnStopped?.Invoke(this);
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
