using System;
using System.Linq;
using UnityEditor;
using UnityEngine.U2D;

namespace Core.Utils
{
    public static class SpriteAtlasUtils
    {
        public static void SetAllIncludeInBuild(bool enable)
        {
            SpriteAtlas[] spriteAtlases = LoadSpriteAtlases();

            foreach (SpriteAtlas atlas in spriteAtlases)
            {
                SetIncludeInBuild(atlas, enable);
            }
        }

        private static void SetIncludeInBuild(SpriteAtlas spriteAtlas, bool enable)
        {
            SerializedObject so = new SerializedObject(spriteAtlas);
            SerializedProperty atlasEditorData = so.FindProperty("m_EditorData");
            SerializedProperty includeInBuild = atlasEditorData.FindPropertyRelative("bindAsDefault");
            includeInBuild.boolValue = enable;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(spriteAtlas);
            AssetDatabase.Refresh();
        }

        private static SpriteAtlas[] LoadSpriteAtlases()
        {
            string[] findAssets = AssetDatabase.FindAssets($"t: {nameof(SpriteAtlas)}");

            if (findAssets.Length == 0)
            {
                return Array.Empty<SpriteAtlas>();
            }

            return findAssets
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<SpriteAtlas>)
                .ToArray();
        }
    }
}