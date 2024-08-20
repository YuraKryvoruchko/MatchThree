using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Core.VFX;

namespace Core.Infrastructure.VFX
{
    public interface IVFXEffectProvider
    {
        UniTask<TVFXEffect> GetVFX<TVFXEffect, TVFXEffectParameters>(AssetReference refrence, TVFXEffectParameters parameters, Vector3 position,
            Quaternion rotation, Transform parent = null) where TVFXEffect : MonoBehaviour, IVFXEffect<TVFXEffectParameters>;

        void ReleaseVFX(AssetReference refrence, GameObject effect);
    }
    public class VFXEffectProvider : IVFXEffectProvider
    {
        private Dictionary<AssetReference, List<GameObject>> _keyValuePairs;

        public VFXEffectProvider()
        {
            _keyValuePairs = new Dictionary<AssetReference, List<GameObject>>();
        }

        public async UniTask<TVFXEffect> GetVFX<TVFXEffect, TVFXEffectParameters>(AssetReference refrence, TVFXEffectParameters parameters, Vector3 position,
            Quaternion rotation, Transform parent = null) where TVFXEffect : MonoBehaviour, IVFXEffect<TVFXEffectParameters>
        {
            GameObject effectObject = (await Addressables.InstantiateAsync(refrence, position, rotation, parent));
            if(effectObject.TryGetComponent(out TVFXEffect effect))
            {
                effect.SetParameters(parameters);
                AddToDictionary(refrence, effectObject);
            }
            else
            {
                throw new System.Exception();
            }

            return effect;
        }
        public void ReleaseVFX(AssetReference refrence, GameObject effect) 
        {
            _keyValuePairs[refrence].Remove(effect);
            Addressables.ReleaseInstance(effect);
        }

        private void AddToDictionary(AssetReference refrence, GameObject effect)
        {
            if (_keyValuePairs.ContainsKey(refrence))
                _keyValuePairs[refrence].Add(effect);
            else
                _keyValuePairs.Add(refrence, new List<GameObject>() { effect });
        }
    }
}
