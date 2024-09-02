using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Infrastructure.Loading
{
    [CreateAssetMenu(fileName = "ResourceList", menuName = "SO/Loading/ResourceList")]
    public class ResourceList : ScriptableObject
    {
        public Category[] SubClasses;

        [Serializable]
        public class Category
        {
#if UNITY_EDITOR
            public string EditorName;
#endif
            public string LoadingDescription;
            public AssetReference[] References;
        }
    }
}
