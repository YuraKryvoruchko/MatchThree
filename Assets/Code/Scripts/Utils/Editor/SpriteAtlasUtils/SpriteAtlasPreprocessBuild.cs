using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Core.Utils
{
    public class SpriteAtlasPreprocessBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder { get; } = int.MinValue;

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("##### SpriteAtlas PreprocessBuild start ######");

            Debug.Log(
                "When loading the sprite atlases via Addressable, we should avoid they getting build into the app. " +
                "Otherwise, the sprite atlases will get loaded into memory twice.");

            Debug.Log("Set the `IncludeInBuild` flag to `false` for all sprite atlases before starting the App build.");
            SpriteAtlasUtils.SetAllIncludeInBuild(false);

            Debug.Log("##### SpriteAtlas PreprocessBuild end ######");
        }
    }
}