using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using UnityEditor;
using UnityEngine;

namespace Silphid.Showzup.Editor
{
    public class ManifestWindow : EditorWindow
    {
        private GUIStyle _normalStyle;
        private GUIStyle _mappingArrowStyle;
        private GUIStyle _explicitVariantStyle;
        private GUIStyle _implicitVariantStyle;
        private string _filter = "";
        private Vector2 _scrollPos;

        [MenuItem("Window/Showzup/Manifest")]
        public static void Open()
        {
            GetWindow(typeof(ManifestWindow), false, "Manifest", true);
        }

        public void OnEnable()
        {
            _normalStyle = CreateLabelStyle(new Color(0.69f, 0.69f, 0.69f), new Color(0.28f, 0.28f, 0.28f));
            _mappingArrowStyle = CreateLabelStyle(new Color(0.83f, 0.68f, 0.52f), new Color(0.6f, 0.49f, 0.37f));
            _explicitVariantStyle = CreateLabelStyle(new Color(0.97f, 0.55f, 0f), new Color(0.67f, 0.35f, 0f));
            _implicitVariantStyle = CreateLabelStyle(new Color(0.74f, 0.38f, 0f), new Color(0f, 0.18f, 0.58f));
        }

        private static GUIStyle CreateLabelStyle(Color proColor, Color normalColor)
        {
            var style = new GUIStyle
            {
                stretchWidth = false,
                fontStyle = FontStyle.Bold,
                margin = new RectOffset(5, 5, 0, 0)
            };

            style.normal.textColor = EditorGUIUtility.isProSkin
                                         ? proColor
                                         : normalColor;
            return style;
        }

        public void OnGUI()
        {
            var manifest = ManifestManager.Manifest;

            // Begin scroll
            EditorGUILayout.BeginVertical();
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            // Build button
            if (GUILayout.Button("Build", GUILayout.ExpandWidth(true)))
                ManifestBuilder.Build();

            // Filter
            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Filter", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
            EditorGUILayout.BeginVertical();
            GUILayout.Space(6);
            _filter = GUILayout.TextField(_filter, GUILayout.MaxWidth(200));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            // Variants
            EditorGUILayout.Separator();
            GUILayout.Label("Variants", EditorStyles.whiteLargeLabel);
            ShowVariants(manifest.AllVariants);

            // Models => View Models
            EditorGUILayout.Separator();
            GUILayout.Label("Models => View Models", EditorStyles.whiteLargeLabel);
            Labels(manifest.ModelsToViewModels);

            // View Models => Views
            EditorGUILayout.Separator();
            GUILayout.Label("View Models => Views", EditorStyles.whiteLargeLabel);
            Labels(manifest.ViewModelsToViews);

            // Views => Prefabs
            EditorGUILayout.Separator();
            GUILayout.Label("Views => Prefabs", EditorStyles.whiteLargeLabel);
            Labels(manifest.ViewsToPrefabs);

            // End scroll
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void ShowVariants(VariantSet manifestAllVariants)
        {
            var groups = manifestAllVariants.GroupBy(x => x.Group)
                                            .OrderBy(x => x.Key.Name);

            foreach (var group in groups)
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label(group.Key.Name, _normalStyle);
                GUILayout.Label(" : ", _mappingArrowStyle);

                var formattedVariants = group.Select(x => $"[{x.Name}]")
                                             .JoinAsString(" ");

                GUILayout.Label(formattedVariants, _explicitVariantStyle);

                EditorGUILayout.EndHorizontal();
            }
        }

        private void Labels(IEnumerable<Mapping> mappings)
        {
            mappings.Where(x => x.Matches(_filter))
                    .ForEach(Label);
        }

        private void Label(Mapping mapping)
        {
            if (mapping is TypeToTypeMapping)
                Label((TypeToTypeMapping) mapping);
            else
                Label((ViewToPrefabMapping) mapping);
        }

        private void Label(TypeToTypeMapping mapping)
        {
            if (mapping == null)
            {
                Debug.Log("Null mapping detected!");
                return;
            }

            if (mapping.Source == null ||
                mapping.Target == null ||
                mapping.Variants == null ||
                mapping.ImplicitVariants == null)
            {
                Debug.Log("Mapping has some null values: " + mapping);
                return;
            }
            
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(mapping.Source.Name, _normalStyle);
            GUILayout.Label(" => ", _mappingArrowStyle);
            GUILayout.Label(mapping.Target.Name, _normalStyle);
            if (mapping.Variants.Any())
                GUILayout.Label($" [{mapping.Variants}]", _explicitVariantStyle);

            if (mapping.ImplicitVariants.Any())
                GUILayout.Label($" ({mapping.ImplicitVariants})", _implicitVariantStyle);

            EditorGUILayout.EndHorizontal();
        }

        private void Label(ViewToPrefabMapping mapping)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(mapping.Source.Name, _normalStyle);
            GUILayout.Label(" => ", _mappingArrowStyle);
            GUILayout.Label(mapping.Target.ToString(), _normalStyle);

            if (mapping.Variants.Any())
                GUILayout.Label($" [{mapping.Variants}]", _explicitVariantStyle);

            EditorGUILayout.EndHorizontal();
        }
    }
}