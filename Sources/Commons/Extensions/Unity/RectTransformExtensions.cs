using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using log4net;
using UnityEngine.UI;

namespace Silphid.Extensions
{
    public static class RectTransformExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RectTransformExtensions));

        public static IEnumerable<T> OfComponentImplementing<T>(this IEnumerable<RectTransform> This)
        {
            return This.SelectMany(g => g.GetComponents<T>());
        }

        public static Vector3 GetCenter(this RectTransform This)
        {
            var corners = new Vector3[4];
            This.GetWorldCorners(corners);
            return corners.Average();
        }

        public static void Stretch(this RectTransform This)
        {
            This.anchorMin = Vector2.zero;
            This.anchorMax = Vector2.one;
            This.sizeDelta = Vector2.zero;
            This.pivot = new Vector2(0.5f, 0.5f);
        }

        public static void StretchFromHeight(this RectTransform This)
        {
            This.anchorMin = Vector2.zero;
            This.anchorMax = Vector2.one;
            This.sizeDelta = Vector2.zero;
            This.pivot = new Vector2(0.5f, 1f);
        }

        public static void StretchHeight(this RectTransform This)
        {
            This.anchorMin = Vector2.zero;
            This.anchorMax = new Vector2(0, 1);
            This.sizeDelta = Vector2.zero;
        }

        public static void AlignLeft(this RectTransform This)
        {
            This.pivot = new Vector2(0, 0.5f);
        }

        /// <summary>
        /// Calculates the absolute position and relative scale to apply to This RectTransform in order to
        /// fit its sourceRect into the targetRect of target RectTransform.  If null or not specified, sourceRect
        /// and targetRect default to entire bounds of their respective RectTransforms.
        /// </summary>
        public static Tuple<Vector3, float> Fit(this RectTransform This,
                                                RectTransform target,
                                                Rect? sourceRect = null,
                                                Rect? targetRect = null)
        {
            // Resolve arguments with default values
            var sourceInner = sourceRect ?? new Rect(0, 0, 1, 1);
            var targetInner = targetRect ?? new Rect(0, 0, 1, 1);

            // Retrieve corners of both transforms (order is bottom-left, top-left, top-right, bottom-right)
            var sourceOuterCorners = new Vector3[4];
            This.GetWorldCorners(sourceOuterCorners);
            var targetOuterCorners = new Vector3[4];
            target.GetWorldCorners(targetOuterCorners);

            // Calculate vectors of top edge (top-left to top-right) and left edge (top-left to bottom-left)
            var sourceOuterTopVector = sourceOuterCorners[2] - sourceOuterCorners[1];
            var targetOuterTopVector = targetOuterCorners[2] - targetOuterCorners[1];
            var targetOuterLeftVector = targetOuterCorners[0] - targetOuterCorners[1];

            // Calculate scale
            var sourceInnerWidth = sourceOuterTopVector.magnitude * sourceInner.width;
            var targetInnerWidth = targetOuterTopVector.magnitude * targetInner.width;
            var scale = targetInnerWidth / sourceInnerWidth;

            // Transpose source pivot into target and convert into world positions
            var sourcePivotWithinSourceInner = (This.pivot - sourceInner.min).Divide(sourceInner.size);
            var fittedPivotWithinTargetOuter =
                sourcePivotWithinSourceInner.Multiply(targetInner.size) + targetInner.min;
            var fittedPivotInWorld = targetOuterCorners[1] + targetOuterTopVector * fittedPivotWithinTargetOuter.x +
                                     targetOuterLeftVector * fittedPivotWithinTargetOuter.y;

            return Tuple.Create(fittedPivotInWorld, scale);
        }

        public static void SetAnchorX(this RectTransform transform, float x)
        {
            var pos = transform.anchoredPosition;
            pos.x = x;
            transform.anchoredPosition = pos;
        }

        public static void SetAnchorY(this RectTransform transform, float y)
        {
            var pos = transform.anchoredPosition;
            pos.y = y;
            transform.anchoredPosition = pos;
        }

        public static bool ContainsPoint(this RectTransform rectTransform, Vector2 point, Camera camera)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, point, camera);
        }

        public static Vector3 ScreenToLocalPoint(this RectTransform rectTransform, Vector2 point, Camera camera)
        {
            var worldPosition = camera.ScreenToWorldPoint(point);
            var delta = rectTransform.anchoredPosition.ToVector3() - rectTransform.transform.position;
            return worldPosition + delta;
        }

        public static void SetDefaultScale(this RectTransform trans)
        {
            trans.localScale = new Vector3(1, 1, 1);
        }

        public static void SetPivotAndAnchors(this RectTransform trans, Vector2 aVec)
        {
            trans.pivot = aVec;
            trans.anchorMin = aVec;
            trans.anchorMax = aVec;
        }

        public static Vector2 GetSize(this RectTransform trans)
        {
            return trans.rect.size;
        }

        public static float GetWidth(this RectTransform trans)
        {
            return trans.rect.width;
        }

        public static float GetHeight(this RectTransform trans)
        {
            return trans.rect.height;
        }

        public static void SetPositionOfPivot(this RectTransform trans, Vector2 newPos)
        {
            trans.localPosition = new Vector3(newPos.x, newPos.y, trans.localPosition.z);
        }

        public static void SetLeftBottomPosition(this RectTransform trans, Vector2 newPos)
        {
            trans.localPosition = new Vector3(
                newPos.x + (trans.pivot.x * trans.rect.width),
                newPos.y + (trans.pivot.y * trans.rect.height),
                trans.localPosition.z);
        }

        public static void SetLeftTopPosition(this RectTransform trans, Vector2 newPos)
        {
            trans.localPosition = new Vector3(
                newPos.x + (trans.pivot.x * trans.rect.width),
                newPos.y - ((1f - trans.pivot.y) * trans.rect.height),
                trans.localPosition.z);
        }

        public static void SetRightBottomPosition(this RectTransform trans, Vector2 newPos)
        {
            trans.localPosition = new Vector3(
                newPos.x - ((1f - trans.pivot.x) * trans.rect.width),
                newPos.y + (trans.pivot.y * trans.rect.height),
                trans.localPosition.z);
        }

        public static void SetRightTopPosition(this RectTransform trans, Vector2 newPos)
        {
            trans.localPosition = new Vector3(
                newPos.x - ((1f - trans.pivot.x) * trans.rect.width),
                newPos.y - ((1f - trans.pivot.y) * trans.rect.height),
                trans.localPosition.z);
        }

        public static void SetSize(this RectTransform trans, Vector2 newSize)
        {
            Vector2 oldSize = trans.rect.size;
            Vector2 deltaSize = newSize - oldSize;
            trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
            trans.offsetMax = trans.offsetMax + new Vector2(
                                  deltaSize.x * (1f - trans.pivot.x),
                                  deltaSize.y * (1f - trans.pivot.y));
        }

        public static void SetWidth(this RectTransform trans, float newSize)
        {
            SetSize(trans, new Vector2(newSize, trans.rect.size.y));
        }

        public static void SetHeight(this RectTransform trans, float newSize)
        {
            SetSize(trans, new Vector2(trans.rect.size.x, newSize));
        }

        public static void SetStretchWithTopOffset(this RectTransform trans, float offset)
        {
            trans.anchoredPosition = trans.anchoredPosition.WithY(-offset * 0.5f);
            trans.sizeDelta = trans.sizeDelta.WithY(-offset);
        }

        public static Vector2Int GetScreenSpacePivot(this RectTransform This, Canvas canvas = null)
        {
            var screenSpaceRect = This.GetScreenSpaceRect(canvas);
            var pivot = This.pivot;
            var pivotX = (int) (screenSpaceRect.x + screenSpaceRect.width * pivot.x);
            var pivotY = (int) (screenSpaceRect.y + (screenSpaceRect.height - screenSpaceRect.height * pivot.y));
            Log.Debug($"screenSpaceRect {screenSpaceRect}");
            Log.Debug($"pivot {pivot}");
            Log.Debug($"pivot transformed {pivotX}x{pivotY}");
            return new Vector2Int(pivotX, pivotY);
        }

        public static Rect GetScreenSpaceRect(this RectTransform transform, Canvas canvas = null)
        {
            var corners = new Vector3[4];
            var screenCorners = new Vector3[2];

            transform.GetWorldCorners(corners);

            foreach (var corner in corners)
                Log.Debug($"Corner : {corner.x}x{corner.y}");
            
            if (canvas != null && (canvas.renderMode == RenderMode.ScreenSpaceCamera ||
                                   canvas.renderMode == RenderMode.WorldSpace))
            {
                screenCorners[0] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[1]);
                Log.Debug($"screenCorners[0] : {screenCorners[0].x}x{screenCorners[0].y}");

                screenCorners[1] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[3]);
            }
            else
            {
                screenCorners[0] = corners[1]
                   .ToVector2();
                screenCorners[1] = corners[3]
                   .ToVector2();
            }

            screenCorners[0]
               .y = Screen.height - screenCorners[0]
                       .y;
            screenCorners[1]
               .y = Screen.height - screenCorners[1]
                       .y;

            return new Rect(screenCorners[0], screenCorners[1] - screenCorners[0]);
        }

        public static Vector2 ActualSize(this RectTransform transform)
        {
            if (transform.GetComponent<Canvas>() != null)
                return transform.rect.size;

            var xEqual = transform.anchorMin.x.IsAlmostEqualTo(transform.anchorMax.x);
            var yEqual = transform.anchorMin.y.IsAlmostEqualTo(transform.anchorMax.y);

            if (xEqual && yEqual)
                return transform.sizeDelta;

            var parentSize = transform.parent.AsRectTransform()
                                      .ActualSize();
            return new Vector2(
                parentSize.x * (transform.anchorMax.x - transform.anchorMin.x) + transform.sizeDelta.x,
                parentSize.y * (transform.anchorMax.y - transform.anchorMin.y) + transform.sizeDelta.y);
        }

        public static Vector2 GetPreferredSize(this RectTransform This) =>
            new Vector2(LayoutUtility.GetPreferredWidth(This), LayoutUtility.GetPreferredHeight(This));

        /// <summary>
        /// Returns Rect of given RectTransform in the coordinates system of target RectTransform.
        /// </summary>
        public static Rect GetRectRelativeTo(this RectTransform This, RectTransform target)
        {
            var size = This.rect.size;
            return new Rect(Vector2.zero.Transform(This, target) - This.pivot * size, size);
        }

        /// <summary>
        /// Returns Rect of given RectTransform in the coordinates system of its parent.
        /// </summary>
        public static Rect GetRectRelativeToParent(this RectTransform This)
        {
            var size = This.rect.size;
            return new Rect(This.anchoredPosition - This.pivot * size, size);
        }
    }
}