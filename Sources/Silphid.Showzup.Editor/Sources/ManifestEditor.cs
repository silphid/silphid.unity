using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Silphid.Showzup
{
    [CustomEditor(typeof(Manifest))]
    public class ManifestEditor : Editor
    {
        private SerializedProperty _prefabsPath;
        private SerializedProperty _uriPrefix;
        private Manifest Target => (Manifest) target; 
    
        [UsedImplicitly]
        private void OnEnable()
        {
            _prefabsPath = serializedObject.FindProperty("PrefabsPath");
            _uriPrefix = serializedObject.FindProperty("UriPrefix");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_prefabsPath);
            EditorGUILayout.PropertyField(_uriPrefix);
            serializedObject.ApplyModifiedProperties();
            
            EditorGUILayout.Separator();
            if (GUILayout.Button("Build", GUILayout.ExpandWidth(true)))
                ManifestBuilder.Build(Target);
            
            EditorGUILayout.Separator();
            GUILayout.Label("Models => View Models", EditorStyles.whiteLargeLabel);
            Target.ModelsToViewModels.ForEach(x => GUILayout.Label(x.ToString()));
            
            EditorGUILayout.Separator();
            GUILayout.Label("View Models => Views", EditorStyles.whiteLargeLabel);
            Target.ViewModelsToViews.ForEach(x => GUILayout.Label(x.ToString()));
            
            EditorGUILayout.Separator();
            GUILayout.Label("Views => Prefabs", EditorStyles.whiteLargeLabel);
            Target.ViewsToPrefabs.ForEach(x => GUILayout.Label(x.ToString()));
        }
    }
}