using System;
using Silphid.Extensions;
using UnityEditor;
using UnityEngine;

namespace Silphid.Showzup.Editor
{
    public class MenuItems
    {
        [MenuItem("GameObject/Showzup/ItemControl", false, 1)]
        public static void CreateItemControl()
        {
            var go = Create<ItemControl>(x => x.Content = x.gameObject);
            Complete(go);
        }

        [MenuItem("GameObject/Showzup/ListControl", false, 2)]
        public static void CreateListControl()
        {
            var go = CreateListControl<ListControl>();
            Complete(go);
        }

        [MenuItem("GameObject/Showzup/TransitionControl", false, 4)]
        private static void CreateTransitionControl()
        {
            var go = CreateTransitionControl<TransitionControl>();
            Complete(go);
        }

        [MenuItem("GameObject/Showzup/NavigationControl", false, 5)]
        private static void CreateNavigationControl()
        {
            var go = CreateTransitionControl<NavigationControl>(
                null,
                x =>
                {
                    x.HistoryContainer = CreateGameObject("History", x.gameObject);
                    x.HistoryContainer.AddComponent<RectTransform>()
                     .Stretch();
                });
            Complete(go);
        }

        [MenuItem("GameObject/Showzup/TabControl", false, 5)]
        private static void CreateTabControl()
        {
            var go = Create<TabControl>(
                x =>
                {
                    x.TabListControl = CreateListControl<ListControl>(x.gameObject)
                       .GetComponent<ListControl>();
                    x.ContentTransitionControl = CreateTransitionControl<TransitionControl>(x.gameObject)
                       .GetComponent<TransitionControl>();
                });
            Complete(go);
        }

        private static GameObject CreateListControl<T>(GameObject parent = null) where T : ListControl
        {
            return Create<T>(parent, x => x.Content = x.gameObject);
        }

        private static GameObject CreateTransitionControl<T>(GameObject parent = null, Action<T> action = null)
            where T : TransitionControl
        {
            return Create<T>(
                parent,
                x =>
                {
                    x.Container1 = CreateGameObject("Container1", x.gameObject);
                    x.Container1.AddComponent<RectTransform>()
                     .Stretch();
                    x.Container1.AddComponent<CanvasGroup>();

                    x.Container2 = CreateGameObject("Container2", x.gameObject);
                    x.Container2.AddComponent<RectTransform>()
                     .Stretch();
                    x.Container2.AddComponent<CanvasGroup>();

                    action?.Invoke(x);
                });
        }

        private static GameObject Create<T>(Action<T> action = null) where T : Component =>
            Create(null, action);

        private static GameObject Create<T>(GameObject parent, Action<T> action = null) where T : Component
        {
            parent = parent ?? Selection.activeObject as GameObject;
            var go = CreateGameObject(typeof(T).Name, parent);

            go.AddComponent<RectTransform>()
              .Stretch();
            var component = go.AddComponent<T>();

            action?.Invoke(component);
            return go;
        }

        private static void Complete(GameObject go)
        {
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