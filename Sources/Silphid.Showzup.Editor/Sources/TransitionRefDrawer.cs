using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using UnityEditor;
using UnityEngine;

namespace Silphid.Showzup
{
    [CustomPropertyDrawer(typeof(TransitionRef))]
    public class TransitionRefDrawer : PropertyDrawer
    {
        private readonly List<Type> _types;
        private readonly string[] _choices;
        
        public TransitionRefDrawer()
        {
            _types = GetAllTypesInAppDomain()
                .Where(x => !x.IsAbstract && x.IsAssignableTo<ITransition>())
                .Prepend(null)
                .ToList();

            _choices = _types
                .Select(x => x == null
                    ? "Default" :
                    x.Name.RemoveSuffix("Transition"))
                .ToArray();
        }
        
        private static IEnumerable<Type> GetAllTypesInAppDomain() =>
            AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes());

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

//            var obj = property.objectReferenceValue;

//            var transitionProperty = property.FindPropertyRelative("Transition");
//            Debug.Log($"Value: {(ITransition)transitionProperty.objectReferenceValue}");

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
//            int selected = 0;//_types.IndexOf(transitionProperty.object)
//            selected = EditorGUI.Popup(position, selected, _choices);
            
            EditorGUI.PropertyField(position, property, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}