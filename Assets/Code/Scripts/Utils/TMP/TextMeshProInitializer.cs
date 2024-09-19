using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class TextMeshProInitializer
{

    private static TextMeshProResourcesAPI resourcesAPI;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void InitializeRuntime()
    {
        RegisterAPI();
    }

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    static void InitializeEditor()
    {
        RegisterAPI();
    }
#endif

    private static void RegisterAPI()
    {

        resourcesAPI = new TextMeshProResourcesAPI();

        ResourcesAPI.overrideAPI = resourcesAPI;
    }
}

public class TextMeshProResourcesAPI : ResourcesAPI
{

    private const string AssetsFolder = "Assets/Plugins/TextMesh Pro/Assets";
    private const string MaterialsFolder = "Materials";
    private const string AssetExtension = ".asset";
    private const string MaterialExtension = ".mat";

    private static readonly Type settingsType = typeof(TMP_Settings);
    private static readonly Type fontAssetType = typeof(TMP_FontAsset);
    private static readonly Type materialType = typeof(Material);
    private static readonly Type spriteAssetType = typeof(TMP_SpriteAsset);

    private bool isLoading = false;
    private Dictionary<int, IResourceLocation> locations = null;
    private List<AsyncOperationHandle> operations = new List<AsyncOperationHandle>(1);

    public TextMeshProResourcesAPI()
        : base()
    {
    }

    public void ReleaseOperations()
    {

        foreach (AsyncOperationHandle op in operations)
        {

            if (op.IsValid())
            {

                Addressables.Release(op);
            }
        }

        operations.Clear();
    }

    protected override UnityEngine.Object Load(string path, Type systemTypeInstance)
    {

        //prevent stack overflow exception if we're already loading through Addressables
        //since when Addressables are set to FastMode in editor, that method calls into this ResourcesAPI when running Addressables.LoadAsset calls

        if (isLoading)
        {
            return base.Load(path, systemTypeInstance);
        }

        //handle specific types

        if (systemTypeInstance == settingsType)
        {
            return LoadSettings(path);
        }
        else if (systemTypeInstance == fontAssetType)
        {
            return LoadAssetFromLocation<TMP_FontAsset>(path, default, AssetExtension);
        }
        else if (systemTypeInstance == spriteAssetType)
        {
            return LoadAssetFromLocation<TMP_SpriteAsset>(path, default, AssetExtension);
        }
        else if (systemTypeInstance == materialType)
        {

            string fontAssetsPath = TMP_Settings.defaultFontAssetPath;

            //check if the material is located in the TMP folder

            if (path.StartsWith(fontAssetsPath, StringComparison.Ordinal))
            {
                return LoadAssetFromLocation<Material>(path, MaterialsFolder, MaterialExtension);
            }
        }

        //not a TMP asset, load default

        return base.Load(path, systemTypeInstance);
    }

    private TMP_Settings LoadSettings(string path)
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            return LoadAssetFromAssetDatabase<TMP_Settings>(path, default, AssetExtension);
        }
#endif
        return LoadAddressableAsset<TMP_Settings>(path);
    }

    private T LoadAssetFromLocation<T>(string path, string subFolder, string extension) where T : UnityEngine.Object
    {

#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            return LoadAssetFromAssetDatabase<T>(path, subFolder, extension);
        }
#endif
        string name = Path.GetFileNameWithoutExtension(path);
        int hash = TMP_TextUtilities.GetSimpleHashCode(name);

        if (locations == null)
        {

            LoadResourceLocations();
        }

        if (locations.TryGetValue(hash, out IResourceLocation location))
        {
            return LoadAddressableAsset<T>(location);
        }

        return default;
    }

    private void LoadResourceLocations()
    {

        var fontsHandle = Addressables.LoadResourceLocationsAsync("tmp_fonts");
        var spritesHandle = Addressables.LoadResourceLocationsAsync("tmp_sprites");

        var fontLocations = fontsHandle.WaitForCompletion();
        var spriteLocations = spritesHandle.WaitForCompletion();

        locations = new Dictionary<int, IResourceLocation>(fontLocations.Count + spriteLocations.Count);

        AddLocations<TMP_FontAsset>(fontLocations, AssetExtension, locations);
        AddLocations<Material>(fontLocations, MaterialExtension, locations);
        AddLocations<TMP_SpriteAsset>(spriteLocations, AssetExtension, locations);

        Addressables.Release(fontsHandle);
        Addressables.Release(spritesHandle);
    }

    private T LoadAddressableAsset<T>(object key) where T : UnityEngine.Object
    {

        isLoading = true;

        AsyncOperationHandle<T> op;

        if (key is IResourceLocation location)
        {

            op = Addressables.LoadAssetAsync<T>(location);
        }
        else
        {

            op = Addressables.LoadAssetAsync<T>(key);
        }

        operations.Add(op);

        T asset = op.WaitForCompletion();

        isLoading = false;

        return asset;
    }

#if UNITY_EDITOR
    private T LoadAssetFromAssetDatabase<T>(string resourcePath, string subFolder, string extension) where T : UnityEngine.Object
    {

        string path = Path.Combine(AssetsFolder, $"{resourcePath}{extension}").Replace('\\', '/');

        if (!string.IsNullOrEmpty(subFolder))
        {

            string fileName = Path.GetFileName(path);
            string parentDir = Path.GetDirectoryName(path);

            path = Path.Combine(parentDir, subFolder, fileName).Replace('\\', '/');
        }
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }
#endif
    private void AddLocations<T>(IList<IResourceLocation> locations, string requiredExtension, Dictionary<int, IResourceLocation> lookup)
    {

        Type type = typeof(T);

        foreach (IResourceLocation location in locations)
        {

            //because TMP_FontAssets contain sub assets of material type
            //we need to check the required type for this location. If it doesn't match don't add the location
            //otherwise, we'd be adding 2 locations for the same asset

            if (location.ResourceType != type)
            {
                continue;
            }

            //also check the extension

            string extension = Path.GetExtension(location.PrimaryKey);

            if (!requiredExtension.Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            //finally, get the name of the asset and hash it

            string name = Path.GetFileNameWithoutExtension(location.PrimaryKey);
            int hash = TMP_TextUtilities.GetSimpleHashCode(name);

            if (!lookup.TryAdd(hash, location))
            {

                Trace.WriteLine($"Trying to register multiple font assets: {location.PrimaryKey} - {location.ResourceType}. Required type: {type}");
            }
        }
    }
}