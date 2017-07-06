using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Silphid.Extensions
{
    public static class GameObjectExtensions
    {
        #region Traversal

        public static GameObject Parent(this GameObject This) =>
            This.transform.Parent();

        public static GameObject Child(this GameObject This, string name) =>
            This.transform.Child(name);

        public static IEnumerable<GameObject> Children(this GameObject This) =>
            This.transform.Children();

        public static IEnumerable<GameObject> SelfAndChildren(this GameObject This) =>
            This.transform.SelfAndChildren();

        public static IEnumerable<GameObject> Ancestors(this GameObject This) =>
            This.transform.Ancestors();

        public static IEnumerable<TComponent> Ancestors<TComponent>(this GameObject This) where TComponent : Component =>
            This.transform.Ancestors<TComponent>();

        public static IEnumerable<GameObject> SelfAndAncestors(this GameObject This) =>
            This.transform.SelfAndAncestors();

        public static IEnumerable<TComponent> SelfAndAncestors<TComponent>(this GameObject This) where TComponent : Component =>
            This.transform.SelfAndAncestors<TComponent>();

        public static IEnumerable<GameObject> Descendants(this GameObject This) =>
            This.transform.Descendants();

        public static IEnumerable<GameObject> SelfAndDescendants(this GameObject This) =>
            This.transform.SelfAndDescendants();

        public static IEnumerable<TComponent> SelfAndDescendants<TComponent>(this GameObject This) where TComponent : Component =>
            This.transform.SelfAndDescendants<TComponent>();

        public static bool IsDescendantOf(this GameObject This, GameObject other) =>
            other != null && (This?.transform.Ancestors().Any(x => x == other) ?? false);

        public static bool IsAncestorOf(this GameObject This, GameObject other) =>
            This != null && (other?.transform.Ancestors().Any(x => x == This) ?? false);

        public static bool IsSelfOrDescendantOf(this GameObject This, GameObject other) =>
            This == other || This.IsDescendantOf(other);

        public static bool IsSelfOrAncestorOf(this GameObject This, GameObject other) =>
            This == other || This.IsAncestorOf(other);

        public static GameObject CommonAncestorWith(this GameObject This, GameObject other) =>
            This.transform.CommonAncestorWith(other?.transform);

        #endregion

        #region Filtering

        public static IEnumerable<GameObject> Named(this IEnumerable<GameObject> This, string name) =>
            This.Where(x => x.name == name);

        public static IEnumerable<GameObject> WithComponent<T>(this IEnumerable<GameObject> This) =>
            This.Where(x => x.GetComponent<T>() != null);

        public static IEnumerable<T> SelectComponents<T>(this IEnumerable<GameObject> This) =>
            This.SelectMany(x => x.GetComponents<T>());

        #endregion

        #region Misc helpers

        public static RectTransform RectTransform(this GameObject This) =>
            This.GetComponent<RectTransform>();

        public static void Destroy(this GameObject This)
        {
            Object.Destroy(This);
        }

        public static string ToHierarchyPath(this GameObject This) =>
            This.SelfAndAncestors().Reverse().ToDelimitedString(" > ");

        #endregion

        #region Selection

        public static void Select(this GameObject This)
        {
            EventSystem.current.SetSelectedGameObject(This);
        }

        public static void Select(this Component This)
        {
            This.gameObject.Select();
        }

        public static void SelectDeferred(this GameObject This)
        {
            Scheduler.MainThreadEndOfFrame.Schedule(This.Select);
        }

        public static void SelectDeferred(this Component This)
        {
            This.gameObject.SelectDeferred();
        }

        public static bool IsSelected(this GameObject This) =>
            EventSystem.current.currentSelectedGameObject == This;

        public static bool IsSelected(this Component This) =>
            This.gameObject.IsSelected();

        public static bool IsDescendantSelected(this GameObject This) =>
            EventSystem.current.currentSelectedGameObject?.IsDescendantOf(This) ?? false;

        public static bool IsSelfOrDescendantSelected(this GameObject This) =>
            EventSystem.current.currentSelectedGameObject?.IsSelfOrDescendantOf(This) ?? false;

        public static bool IsDescendantSelected(this Component This) =>
            This.gameObject.IsDescendantSelected();
        
        public static bool IsSelfOrDescendantSelected(this Component This) =>
            This.gameObject.IsSelfOrDescendantSelected();

        #endregion
    }
}