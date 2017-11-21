using System.IO;
using UnityEditor;
using UnityEngine;

public static class ScriptableObjectUtility
{
    public static void Create<T>(string name = null) where T : ScriptableObject
    {
        var asset = ScriptableObject.CreateInstance<T>();
 
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "") 
            path = "Assets";
        else if (Path.GetExtension (path) != "") 
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");

        name = name ?? typeof(T).Name;
        var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"{path}/{name}.asset");
 
        AssetDatabase.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}