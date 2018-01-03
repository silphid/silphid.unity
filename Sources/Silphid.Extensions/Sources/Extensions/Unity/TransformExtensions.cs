using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Silphid.Extensions
{
    public static class TransformExtensions
    {
        #region Traversal

        public static TComponent GetRequiredComponent<TComponent>(this Transform This) =>
            This.gameObject.GetRequiredComponent<TComponent>();

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

        public static IEnumerable<TComponent> Ancestors<TComponent>(this Transform This) =>
            This.Ancestors()
                .SelectMany(x => x.GetComponents<TComponent>())
                .WhereNotNull();

        public static IEnumerable<GameObject> SelfAndAncestors(this Transform This)
        {
            yield return This.gameObject;
            foreach (var ancestor in This.Ancestors())
                yield return ancestor.gameObject;
        }

        public static IEnumerable<TComponent> SelfAndAncestors<TComponent>(this Transform This) =>
            This.SelfAndAncestors()
                .SelectMany(x => x.GetComponents<TComponent>())
                .WhereNotNull();

        public static IEnumerable<GameObject> Descendants(this Transform This)
        {
            foreach (var child in This.ChildTransforms())
            {
                yield return child.gameObject;
                foreach (var descendant in child.Descendants())
                    yield return descendant;
            }
        }

        public static IEnumerable<TComponent> Descendants<TComponent>(this Transform This) =>
            This.Descendants()
                .SelectMany(x => x.GetComponents<TComponent>())
                .WhereNotNull();

        public static IEnumerable<GameObject> SelfAndDescendants(this Transform This)
        {
            yield return This.gameObject;
            foreach (var child in This.Descendants())
                yield return child.gameObject;
        }

        public static IEnumerable<TComponent> SelfAndDescendants<TComponent>(this Transform This) =>
            This.SelfAndDescendants()
                .SelectMany(x => x.GetComponents<TComponent>())
                .WhereNotNull();

        public static GameObject CommonAncestorWith(this Transform This, Transform other)
        {
            if (This == null || other == null)
                return null;
            
            if (This == other)
                return This.gameObject;
            
            var list1 = This.SelfAndAncestors().ToArray();
            var list2 = other.SelfAndAncestors().ToArray();

            GameObject common = null;
            for (int i1 = list1.Length - 1, i2 = list2.Length - 1; i1 >= 0 && i2 >= 0; i1--, i2--)
            {
                var item1 = list1[i1];
                var item2 = list2[i2];

                if (item1 != item2)
                    break;

                common = item1;
            }

            return common;
        }

        public static Tuple<List<GameObject>, List<GameObject>> DivergingBranchesWith(this Transform This, Transform other)
        {
            if (This == null || other == null)
                return null;
            
            if (This == other)
                return Tuple.Create(new List<GameObject>(), new List<GameObject>());
            
            var list1 = This.SelfAndAncestors().ToList();
            var list2 = other.SelfAndAncestors().ToList();

            for (int i1 = list1.Count - 1, i2 = list2.Count - 1; i1 >= 0 && i2 >= 0; i1--, i2--)
            {
                var item1 = list1[i1];
                var item2 = list2[i2];

                if (item1 != item2)
                    break;
                
                list1.RemoveAt(i1);
                list2.RemoveAt(i2);
            }

            return Tuple.Create(list1, list2);
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