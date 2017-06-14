using System;
using Silphid.Extensions;
using UnityEditor;
using UnityEngine;

namespace Silphid.Showzup
{
    public class MenuItems
    {
        [MenuItem("GameObject/Showzup/ItemControl", false, 1)]
        public static void CreateItemControl()
        {
            Create<ItemControl>(x => x.Container = x.gameObject);
        }

        [MenuItem("GameObject/Showzup/ListControl", false, 2)]
        public static void CreateListControl()
        {
            CreateListControl<ListControl>();
        }
                
        [MenuItem("GameObject/Showzup/SelectionControl", false, 3)]
        private static void CreateSelectionControl()
        {
            CreateListControl<SelectionControl>();
        }

        [MenuItem("GameObject/Showzup/TransitionControl", false, 4)]
        private static void CreateTransitionControl()
        {
            CreateTransitionControl<TransitionControl>();
        }
        
        [MenuItem("GameObject/Showzup/NavigationControl", false, 5)]
        private static void CreateNavigationControl()
        {
            CreateTransitionControl<NavigationControl>(x =>
                x.HistoryContainer = CreateGameObject("History", x.gameObject));
        }
        
        private static void CreateListControl<T>() where T : ListControl
        {
            Create<T>(x => x.Container = x.gameObject);
        }
        
        private static void CreateTransitionControl<T>(Action<T> action = null) where T : TransitionControl
        {
            Create<T>(x =>
            {
                x.Container1 = CreateGameObject("Container1", x.gameObject);
                x.Container2 = CreateGameObject("Container2", x.gameObject);
                
                if (action != null)
                    action(x);
            });
        }

        private static void Create<T>(Action<T> action = null) where T : Component
        {
            var parent = Selection.activeObject as GameObject;
            var go = CreateGameObject(typeof(T).Name, parent);

            go.AddComponent<RectTransform>().Stretch();
            
            var component = go.AddComponent<T>();
            if (action != null)
                action(component);

            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        private static GameObject CreateGameObject(string name, GameObject parent)
        {
            var go = new GameObject(name);
            GameObjectUtility.SetParentAndAlign(go, parent);
            return go;
        }
    }
}