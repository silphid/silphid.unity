using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Silphid.Extensions;
using Silphid.Showzup;
using UnityEditor;
using UnityEngine;
using Rx = UniRx;

public class ManifestBuilder
{
    [MenuItem("Assets/Showzup/Update Manifest")]
    private static void UpdateManifest()
    {
        var manifest = LoadManifest();
        if (manifest == null)
        {
            Debug.Log("No manifest to build. Create a new manifest first from Assets/Create/Showzup menu.");
            return;
        }

        var allVariants = GetVariantsFromAllAssemblies();
        Debug.Log($"Detected the following variants: {allVariants.ToDelimitedString(", ")}");

        MapModelsToViewModels(manifest, allVariants);
        MapViewModelsToViews(manifest, allVariants);
        MapViewsToPrefabs(manifest, allVariants);
        
        AssetDatabase.SaveAssets();
    }

    private static Manifest LoadManifest()
    {
        var guids = AssetDatabase.FindAssets($"t:{typeof(Manifest)}");
        if (!guids.Any())
            return null;

        string assetPath = AssetDatabase.GUIDToAssetPath(guids.FirstOrDefault());
        return AssetDatabase.LoadAssetAtPath<Manifest>(assetPath);
    }

    #region ModelsToViewModels

    private static void MapModelsToViewModels(Manifest manifest, VariantSet allVariants)
    {
        manifest.ModelsToViewModels.Clear();

        GetAllTypesInAppDomain()
            .Where(type => type.IsAssignableTo<IViewModel>() && !type.IsAbstract)
            .ForEach(viewModelType => MapModelToViewModel(
                manifest,
                GetModelForViewModel(viewModelType), viewModelType,
                allVariants));
    }

    private static Type GetModelForViewModel(Type viewModelType)
    {
        try
        {
            return viewModelType.GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IViewModel<>))
                .GetGenericArguments()
                .First();
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to resolve Model for ViewModel: {viewType.Name}", ex);
        }
    }
    
    private static void MapModelToViewModel(Manifest manifest, Type modelType, Type viewModelType, VariantSet allVariants)
    {
        var variants = GetVariantsFromTypes(modelType, viewModelType, allVariants);
        Debug.Log($"Mapping Model {modelType} => ViewModel {viewModelType} (Variants: {variants})");
        manifest.ViewModelsToViews.Add(new TypeToTypeMapping(modelType, viewModelType, variants));
    }

    #endregion

    #region ViewModelsToViews

    private static void MapViewModelsToViews(Manifest manifest, VariantSet allVariants)
    {
        manifest.ViewModelsToViews.Clear();

        GetAllTypesInAppDomain()
            .Where(type => type.IsAssignableTo<IView>() && !type.IsAbstract)
            .ForEach(viewType => MapViewModelToView(
                manifest,
                GetViewModelForView(viewType), viewType,
                allVariants));
    }

    private static Type GetViewModelForView(Type viewType)
    {
        try
        {
            return viewType.GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IView<>))
                .GetGenericArguments()
                .First();
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to determine ViewModel for View: {viewType.Name}", ex);
        }
    }
    
    private static void MapViewModelToView(Manifest manifest, Type viewModelType, Type viewType, VariantSet allVariants)
    {
        var variants = GetVariantsFromTypes(viewModelType, viewType, allVariants);
        Debug.Log($"Mapping ViewModel {viewModelType} => View {viewType} (Variants: {variants})");
        manifest.ViewModelsToViews.Add(new TypeToTypeMapping(viewModelType, viewType, variants));
    }

    #endregion

    #region ViewsToPrefabs

    private static void MapViewsToPrefabs(Manifest manifest, VariantSet allVariants)
    {
        manifest.ViewsToPrefabs.Clear();

        var guids = AssetDatabase.FindAssets("t:GameObject", new[] {manifest.PrefabsPath});
        if (guids.Any())
            guids.ForEach(x => MapViewsToPrefabWithGuid(x, manifest, allVariants));
        else
            Debug.Log($"No view prefab could be found in path: {manifest.PrefabsPath}");
    }

    private static void MapViewsToPrefabWithGuid(string guid, Manifest manifest, VariantSet allVariants)
    {
        string prefabPath = AssetDatabase.GUIDToAssetPath(guid);

        var relativePath = GetRelativePrefabPath(prefabPath, manifest.PrefabsPath);

        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var views = asset.GetComponents<IView>().ToList();
        if (!views.Any())
        {
            Debug.Log($"Skipping non-view prefab: {prefabPath}");
            return;
        }

        var assetVariants = GetVariantsFromRelativePath(relativePath, manifest, allVariants);
        
        foreach (var view in views)
            MapViewToRelativePath(view.GetType(), relativePath, assetVariants, allVariants, manifest);
    }

    private static void MapViewToRelativePath(Type viewType, string relativePath, VariantSet assetVariants, VariantSet allVariants,
        Manifest manifest)
    {
        var viewVariants = GetVariantsFromType(viewType, allVariants);
        var variants = viewVariants.UnionWith(assetVariants);
        var uri = GetUriFromRelativePath(relativePath, manifest.UriPrefix);
        
        Debug.Log($"Mapping View {viewType} => Prefab {uri} (Variants: {variants})");
        manifest.ViewsToPrefabs.Add(new TypeToUriMapping(viewType, uri, variants));
    }

    private static string GetRelativePrefabPath(string prefabPath, string pathToPrefabsInAssets)
    {
        if (!pathToPrefabsInAssets.EndsWith("/"))
            pathToPrefabsInAssets += "/";
        
        Debug.Assert(prefabPath.StartsWith(pathToPrefabsInAssets),
            $"Prefab {prefabPath} should be located within {pathToPrefabsInAssets}");
        
        return prefabPath.RemovePrefix(pathToPrefabsInAssets);
    }

    private static Uri GetUriFromRelativePath(string relativePath, string uriPrefix)
    {
        relativePath = relativePath.RemoveSuffix("/");
        var extension = Path.GetExtension(relativePath);
        relativePath = relativePath.RemoveSuffix(extension);
        return new Uri(uriPrefix + relativePath);
    }

    #endregion

    #region Variants
    
    private static VariantSet GetVariantsFromRelativePath(string relativePath, Manifest manifest, VariantSet allVariants)
    {
        var allTokens = relativePath
            .Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

        var variantNames = allTokens
            .Take(allTokens.Length - 1)
            .Select(x => x.ToLower())
            .ToList();

        return GetVariantsFromNames(variantNames, allVariants);
    }

    private static VariantSet GetVariantsFromNames(List<string> names, VariantSet allVariants)
    {
        return allVariants
            .Where(x => names.Contains(x.Name.ToLower()))
            .ToVariantSet();
    }

    private static VariantSet GetVariantsFromAllAssemblies()
    {
        return GetAllTypesInAppDomain()
            .Where(type => type.IsAssignableTo<IVariant>())
            .Select(type => type.GetField("Group", BindingFlags.Static | BindingFlags.Public))
            .Where(field => field != null && field.FieldType.IsAssignableTo<IVariantGroup>())
            .SelectMany(field => ((IVariantGroup) field.GetValue(null)).Variants)
            .ToVariantSet();
    }

    #endregion
    
    #region Helpers

    private static IEnumerable<Type> GetAllTypesInAppDomain()
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes());
    }

    private static VariantSet GetVariantsFromType(Type type, VariantSet allVariants)
    {
        var attributes = type
            .GetAttributes<VariantAttribute>()
            .Select(x => x.Variant)        
            .ToList();
        
        if (!attributes.Any())
            return new VariantSet();

        return allVariants
            .Where(variant => attributes.Contains(variant.Name))
            .ToVariantSet();
    }

    private static VariantSet GetVariantsFromTypes(Type type1, Type type2, VariantSet allVariants)
    {
        return GetVariantsFromType(type1, allVariants)
            .UnionWith(GetVariantsFromType(type2, allVariants));
    }

    #endregion
}