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
        public void OnEnable()
        {
            _prefabsPath = serializedObject.FindProperty("PrefabsPath");
            _uriPrefix = serializedObject.FindProperty("UriPrefix");
        }
        
        public override void OnInspectorGUI()
        {
            // Fields
            serializedObject.Update();
            EditorGUILayout.PropertyField(_prefabsPath);
            EditorGUILayout.PropertyField(_uriPrefix);
            serializedObject.ApplyModifiedProperties();

            // Build button
            EditorGUILayout.Separator();
            if (GUILayout.Button("Build", GUILayout.ExpandWidth(true)))
                ManifestBuilder.Build(Target);
        }
    }
}