using System.Collections.Generic;
using UnityEngine;

namespace Silphid.Extensions
{
    public static class ComponentExtensions
    {
        #region Traversal

        public static GameObject Parent(this Component This) =>
            This.transform.Parent();

        public static GameObject Child(this Component This, string name) =>
            This.transform.Child(name);

        public static IEnumerable<GameObject> Children(this Component This) =>
            This.transform.Children();

        public static IEnumerable<GameObject> SelfAndChildren(this Component This) =>
            This.transform.SelfAndChildren();

        public static IEnumerable<GameObject> Ancestors(this Component This) =>
            This.transform.Ancestors();

        public static IEnumerable<TComponent> Ancestors<TComponent>(this Component This) where TComponent : Component =>
            This.transform.Ancestors<TComponent>();

        public static IEnumerable<GameObject> SelfAndAncestors(this Component This) =>
            This.transform.SelfAndAncestors();

        public static IEnumerable<TComponent> SelfAndAncestors<TComponent>(this Component This) where TComponent : Component =>
            This.transform.SelfAndAncestors<TComponent>();

        public static IEnumerable<GameObject> Descendants(this Component This) =>
            This.transform.Descendants();

        public static IEnumerable<GameObject> SelfAndDescendants(this Component This) =>
            This.transform.SelfAndDescendants();

        public static IEnumerable<TComponent> SelfAndDescendants<TComponent>(this Component This) where TComponent : Component =>
            This.transform.SelfAndDescendants<TComponent>();

        public static GameObject CommonAncestorWith(this Component This, Component other) =>
            This.transform.CommonAncestorWith(other?.transform);

        #endregion
    }
}