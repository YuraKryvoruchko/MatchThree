using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

using SceneInstance = UnityEngine.ResourceManagement.ResourceProviders.SceneInstance;

namespace Core.Infrastructure.Service
{
    public class SceneService
    {
        private Dictionary<string, AsyncOperationHandle<SceneInstance>> _GUIDkeySceneDictionary;

        public SceneService()
        {
            _GUIDkeySceneDictionary = new Dictionary<string, AsyncOperationHandle<SceneInstance>>();
        }

        public AsyncOperationHandle<SceneInstance> LoadSceneAsync(string GUIDKey, LoadSceneMode mode = LoadSceneMode.Single, 
            bool activateOnLoad = true, int priority = 100)
        {
            AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(GUIDKey, mode, activateOnLoad, priority);
            _GUIDkeySceneDictionary.Add(GUIDKey, handle);

            return handle;
        }
        public AsyncOperationHandle<SceneInstance> UnloadSceneAsync(string GUIDKey, 
            UnloadSceneOptions mode = UnloadSceneOptions.None, bool autoReleaseHandle = true)
        {
            if (!_GUIDkeySceneDictionary.ContainsKey(GUIDKey))
                throw new System.Exception($"Scene by key: {GUIDKey} is not loaded!");

            AsyncOperationHandle<SceneInstance> handle = Addressables.UnloadSceneAsync(_GUIDkeySceneDictionary[GUIDKey],
                mode, autoReleaseHandle);
            _GUIDkeySceneDictionary.Remove(GUIDKey);
            return handle;
        }
    }
}
