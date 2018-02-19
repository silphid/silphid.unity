using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Silphid.Extensions;
using Silphid.Showzup;
using UnityEditor;
using UnityEngine;

public class ManifestBuilder
{
    private static readonly Regex ExcludedNamespacesRegex = new Regex(@"\bTest\b");
    
    [MenuItem("Assets/Build Showzup Manifest %#&s")]
    public static void Build()
    {
        var manifest = ManifestManager.Manifest;
        Build(manifest);
    }

    public static void Build(Manifest manifest)
    {
        manifest.AllVariants = GetVariantsFromAllAssemblies();

        MapModelsToViewModels(manifest);
        MapViewModelsToViews(manifest);
        MapViewsToPrefabs(manifest);
        PropagateImplicitVariants(manifest);

        EditorUtility.SetDirty(manifest);
        AssetDatabase.SaveAssets();

        EditorGUIUtility.PingObject(manifest);
        Selection.activeObject = manifest;
        ManifestWindow.Open();
    }

    #region ModelsToViewModels

    private static void MapModelsToViewModels(Manifest manifest)
    {
        manifest.ModelsToViewModels.Clear();

        GetAllTypesInAppDomain()
            .Where(type => type.IsAssignableTo<IViewModel>() && !type.IsAbstract && !type.IsGenericType)
            .ForEach(viewModelType => MapModelToViewModel(
                manifest,
                GetModelForViewModel(viewModelType), viewModelType,
                manifest.AllVariants));
        
        manifest.ModelsToViewModels.Sort(MappingSortingComparison);
    }

    private static Comparison<Mapping> MappingSortingComparison =>
        (x, y) => string.Compare(x.Source.Name, y.Source.Name, StringComparison.Ordinal);

    private static Type GetModelForViewModel(Type viewModelType) =>
        viewModelType.GetInterfaces()
            .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IViewModel<>))
            ?.GetGenericArguments()
            .FirstOrDefault();

    private static void MapModelToViewModel(Manifest manifest, Type modelType, Type viewModelType, VariantSet allVariants)
    {
        if (modelType == null)
            return;
        
        var variants = GetVariantsFromTypes(modelType, viewModelType, allVariants);
        var mapping = new TypeToTypeMapping(modelType, viewModelType, variants);
        manifest.ModelsToViewModels.Add(mapping);
    }

    #endregion

    #region ViewModelsToViews

    private static void MapViewModelsToViews(Manifest manifest)
    {
        manifest.ViewModelsToViews.Clear();

        GetAllTypesInAppDomain()
            .Where(type => type.IsAssignableTo<IView>() && !type.IsAbstract)
            .ForEach(viewType => MapViewModelToView(
                manifest,
                GetViewModelForView(viewType), viewType,
                manifest.AllVariants));
        
        manifest.ViewModelsToViews.Sort(MappingSortingComparison);
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
        var mapping = new TypeToTypeMapping(viewModelType, viewType, variants);
        manifest.ViewModelsToViews.Add(mapping);
    }

    #endregion

    #region ViewsToPrefabs

    private static void MapViewsToPrefabs(Manifest manifest)
    {
        manifest.ViewsToPrefabs.Clear();

        var guids = AssetDatabase.FindAssets("t:GameObject", new[] {manifest.PrefabsPath});
        if (guids.Any())
            guids.ForEach(x => MapViewsToPrefabWithGuid(x, manifest, manifest.AllVariants));
        else
            Debug.Log($"No view prefab could be found in path: {manifest.PrefabsPath}");
                        
        manifest.ViewsToPrefabs.Sort(MappingSortingComparison);
    }

    private static void MapViewsToPrefabWithGuid(string guid, Manifest manifest, VariantSet allVariants)
    {
        string prefabPath = AssetDatabase.GUIDToAssetPath(guid);

        var relativePath = GetRelativePrefabPath(prefabPath, manifest.PrefabsPath);

        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var views = asset.GetComponents<IView>().ToList();
        if (!views.Any())
        {
            Debug.LogWarning($"Skipping non-view prefab: {prefabPath}");
            return;
        }

        var assetVariants = GetVariantsFromRelativePath(relativePath, manifest, allVariants);

        foreach (var view in views)
            MapViewToRelativePath(view.GetType(), relativePath, guid, assetVariants, allVariants, manifest);
    }

    private static void MapViewToRelativePath(Type viewType, string relativePath, string guid,
        VariantSet assetVariants, VariantSet allVariants, Manifest manifest)
    {
        var viewVariants = GetVariantsFromType(viewType, allVariants);
        var variants = viewVariants.UnionWith(assetVariants);
        var uri = GetUriFromRelativePath(relativePath, manifest.UriPrefix);

        var mapping = new ViewToPrefabMapping(viewType, uri, guid, variants);
        manifest.ViewsToPrefabs.Add(mapping);
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
            .Where(type =>
                !type.IsGenericType &&
                !type.IsAbstract &&
                type.IsAssignableTo<IVariant>())
            .SelectMany(type => type.GetFields(BindingFlags.Static | BindingFlags.Public))
            .Where(field => field.FieldType.IsAssignableTo<IVariant>())
            .Select(field => (IVariant) field.GetValue(null))
            .ToVariantSet();
    }

    #endregion

    #region Implicit variants

    private static void PropagateImplicitVariants(Manifest manifest)
    {
        manifest.ViewModelsToViews.ForEach(vmToV =>
        {
            vmToV.ImplicitVariants = manifest.ViewsToPrefabs
                .Where(vToP => vmToV.Target.IsAssignableTo(vToP.Source))
                .Aggregate(VariantSet.Empty, (acc, vToP) => acc
                    .UnionWith(vToP.Variants));
        });
        
        manifest.ModelsToViewModels.ForEach(vToVm =>
        {
            vToVm.ImplicitVariants = manifest.ViewModelsToViews
                .Where(vmToV => vToVm.Target.IsAssignableTo(vmToV.Source))
                .Aggregate(VariantSet.Empty, (acc, vToP) => acc
                    .UnionWith(vToP.Variants)
                    .UnionWith(vToP.ImplicitVariants));
        });
    }

    #endregion
    
    #region Helpers

    private static IEnumerable<Type> GetAllTypesInAppDomain() =>
        AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly =>
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch
                {
                    Debug.LogWarning($"Failed to load types from assembly: {assembly.GetName().Name}");
                    return new Type[] { };
                }
            })
            .Where(type => !ExcludedNamespacesRegex.IsMatch(type?.Namespace ?? ""));

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