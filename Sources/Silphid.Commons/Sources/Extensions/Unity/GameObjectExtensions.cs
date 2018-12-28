using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Silphid.Extensions
{
    public static class GameObjectExtensions
    {
        #region Traversal

        public static TComponent GetRequiredComponent<TComponent>(this GameObject This)
        {
            var component = This.GetComponent<TComponent>();
            if (component == null)
                throw new InvalidOperationException($"GameObject is required to have a {typeof(TComponent).Name} component: {This.gameObject.ToHierarchyPath()}");

            return component;
        }

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

        public static IEnumerable<TComponent> Ancestors<TComponent>(this GameObject This) =>
            This.transform.Ancestors<TComponent>();

        public static IEnumerable<GameObject> SelfAndAncestors(this GameObject This) =>
            This.transform.SelfAndAncestors();

        public static IEnumerable<TComponent> SelfAndAncestors<TComponent>(this GameObject This) =>
            This.transform.SelfAndAncestors<TComponent>();

        public static IEnumerable<GameObject> Descendants(this GameObject This) =>
            This.transform.Descendants();

        public static IEnumerable<TComponent> Descendants<TComponent>(this GameObject This) =>
            This.transform.Descendants<TComponent>();

        public static IEnumerable<GameObject> SelfAndDescendants(this GameObject This) =>
            This.transform.SelfAndDescendants();

        public static IEnumerable<TComponent> SelfAndDescendants<TComponent>(this GameObject This) =>
            This.transform.SelfAndDescendants<TComponent>();

        public static bool IsDescendantOf(this GameObject This, GameObject other) =>
            other != null && This && This.transform.Ancestors().Any(x => x == other);

        public static bool IsAncestorOf(this GameObject This, GameObject other) =>
            This != null && other && other.transform.Ancestors().Any(x => x == This);

        public static bool IsSelfOrDescendantOf(this GameObject This, GameObject other) =>
            This == other || This.IsDescendantOf(other);

        public static bool IsSelfOrAncestorOf(this GameObject This, GameObject other) =>
            This == other || This.IsAncestorOf(other);

        public static GameObject CommonAncestorWith(this GameObject This, GameObject other) =>
            This.transform.CommonAncestorWith(other.transform);

        public static (List<GameObject>, List<GameObject>) DivergingBranchesWith(this GameObject This, GameObject other) =>
            This.transform.DivergingBranchesWith(other.transform);
        
        #endregion

        #region Filtering

        public static IEnumerable<GameObject> Named(this IEnumerable<GameObject> This, string name) =>
            This.Where(x => x.name == name);

        public static IEnumerable<GameObject> WithComponent<T>(this IEnumerable<GameObject> This) =>
            This.Where(x => x.GetComponent<T>() != null);

        public static IEnumerable<T> OfComponent<T>(this IEnumerable<GameObject> This) =>
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
            This.SelfAndAncestors()
                .Reverse()
                .Select(x => x.name)
                .JoinAsString(" > ");

        #endregion
    }
}