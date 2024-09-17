using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Core.Utils
{
    public class SpriteAtlasPostprocessBuild : IPostprocessBuildWithReport
    {
        public int callbackOrder { get; } = 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            Debug.Log(
                "When loading the sprite atlases via Addressable, we should avoid they getting build into the app. " +
                "Otherwise, the sprite atlases will get loaded into memory twice.");

            Debug.Log("Set the `IncludeInBuild` flag to `true` for all sprite atlases after finishing the App build.");
            SpriteAtlasUtils.SetAllIncludeInBuild(true);
        }
    }
}