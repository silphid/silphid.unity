using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Silphid.Extensions
{
    public static class TransformExtensions
    {
        #region Traversal

        public static GameObject Parent(this Transform This) =>
            This.parent?.gameObject;

        public static GameObject Child(this Transform This, string name) =>
            (from Transform t in This
                where t.name == name
                select t.gameObject)
            .FirstOrDefault();

        public static IEnumerable<GameObject> Children(this Transform This) =>
            from Transform child in This
            select child.gameObject;

        public static IEnumerable<GameObject> SelfAndChildren(this Transform This)
        {
            yield return This.gameObject;
            foreach (var child in This.Children())
                yield return child.gameObject;
        }

        private static IEnumerable<Transform> ChildTransforms(this Transform This) =>
            from Transform child in This
            select child;

        public static IEnumerable<GameObject> Ancestors(this Transform This)
        {
            var ancestor = This.parent;
            while (ancestor != null)
            {
                yield return ancestor.gameObject;
                ancestor = ancestor.parent;
            }
        }

        public static IEnumerable<TComponent> Ancestors<TComponent>(this Transform This) where TComponent : Component =>
            This.Ancestors()
                .SelectMany(x => x.GetComponents<TComponent>());

        public static IEnumerable<GameObject> SelfAndAncestors(this Transform This)
        {
            yield return This.gameObject;
            foreach (var ancestor in This.Ancestors())
                yield return ancestor.gameObject;
        }

        public static IEnumerable<GameObject> Descendants(this Transform This)
        {
            foreach (var child in This.ChildTransforms())
            {
                yield return child.gameObject;
                foreach (var descendant in child.Descendants())
                    yield return descendant;
            }
        }

        public static IEnumerable<GameObject> SelfAndDescendants(this Transform This)
        {
            yield return This.gameObject;
            foreach (var child in This.Descendants())
                yield return child.gameObject;
        }

        public static void SetX(this Transform This, float x)
        {
            var pos = This.position;
            pos.x = x;
            This.position = pos;
        }

        #endregion

        #region Setters

        public static void SetY(this Transform This, float y)
        {
            var pos = This.position;
            pos.y = y;
            This.position = pos;
        }

        public static void SetZ(this Transform This, float z)
        {
            var pos = This.position;
            pos.z = z;
            This.position = pos;
        }

        public static void SetLocalX(this Transform This, float x)
        {
            var pos = This.localPosition;
            pos.x = x;
            This.localPosition = pos;
        }

        public static void SetLocalY(this Transform This, float y)
        {
            var pos = This.localPosition;
            pos.y = y;
            This.localPosition = pos;
        }

        public static void SetLocalZ(this Transform This, float z)
        {
            var pos = This.localPosition;
            pos.z = z;
            This.localPosition = pos;
        }

        #endregion

        public static RectTransform AsRectTransform(this Transform This)
        {
            return (RectTransform)This;
        }

        public static Rect Bounds(this Transform This)
        {
            var rectTransform = This.AsRectTransform();
            var rect = rectTransform.rect;
            var pos = rectTransform.position;

            var left = pos.x - rectTransform.pivot.x * rectTransform.rect.width;
            var bottom = pos.y - rectTransform.pivot.y * rectTransform.rect.height;

            var rect2 = new Rect(left, bottom, rect.width, rect.height);
            return rect2;
        }

        public static float Height(this Transform This)
        {
            return This.AsRectTransform().rect.height;
        }

        public static float Width(this Transform This)
        {
            return This.AsRectTransform().rect.width;
        }

        public static void TranslateAndClamp(this Transform This, Vector2 vector)
        {
            var parentBound = This.parent.Bounds();

            var bounds = This.Bounds();
            var bounds2 = bounds.Translate(vector);

            if (bounds2.xMin < parentBound.xMin)
                vector.x += (parentBound.xMin - bounds2.xMin);
            if (bounds2.yMin < parentBound.yMin)
                vector.y += (parentBound.yMin - bounds2.yMin);
            if (bounds2.xMax > parentBound.xMax)
                vector.x += (parentBound.xMax - bounds2.xMax);

            This.Translate(vector);
        }
    }
}