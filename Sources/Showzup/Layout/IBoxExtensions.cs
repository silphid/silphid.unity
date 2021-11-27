using System;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup.Layout
{
    public static class IBoxExtensions
    {
        public static void SetMin(this IBox This, Vector2 value)
        {
            This.XMin.Value = value.x;
            This.YMin.Value = value.y;
        }

        public static void SetMin(this IBox This, float x, float y)
        {
            This.XMin.Value = x;
            This.YMin.Value = y;
        }

        public static void SetMax(this IBox This, Vector2 value)
        {
            This.XMax.Value = value.x;
            This.YMax.Value = value.y;
        }

        public static void SetMax(this IBox This, float x, float y)
        {
            This.XMax.Value = x;
            This.YMax.Value = y;
        }

        public static void SetRect(this IBox This, Rect value)
        {
            This.XMin.Value = value.xMin;
            This.YMin.Value = value.yMin;
            This.XMax.Value = value.xMax;
            This.YMax.Value = value.yMax;
        }

        public static void SetTopDownRect(this IBox This, Rect value)
        {
            This.XMin.Value = value.xMin;
            This.YMin.Value = -value.yMax;
            This.XMax.Value = value.xMax;
            This.YMax.Value = -value.yMin;
        }

        public static void SetRect(this IBox This, float x, float y, float width, float height)
        {
            This.XMin.Value = x;
            This.YMin.Value = y;
            This.Width.Value = width;
            This.Height.Value = height;
        }

        public static void SetSize(this IBox This, Vector2 value)
        {
            This.Width.Value = value.x;
            This.Height.Value = value.y;
        }

        public static Vector2 GetMin(this IBox This) =>
            new Vector2(This.XMin.Value, This.YMin.Value);

        public static Vector2 GetMax(this IBox This) =>
            new Vector2(This.XMax.Value, This.YMax.Value);

        public static Rect GetRect(this IBox This) =>
            new Rect(This.XMin.Value, This.YMin.Value, This.Width.Value, This.Height.Value);

        public static Rect GetTopDownRect(this IBox This) =>
            new Rect(This.XMin.Value, -This.YMax.Value, This.Width.Value, This.Height.Value);

        public static Vector2 GetSize(this IBox This) =>
            new Vector2(This.Width.Value, This.Height.Value);

        public static IDisposable Bind(this IBox This, Alignment horizontal, Alignment vertical) =>
            new CompositeDisposable(This.BindHorizontal(horizontal), This.BindVertical(vertical));

        public static IDisposable Bind(this IBox This, Alignment alignment) =>
            This.Bind(alignment, alignment);

        public static IDisposable BindHorizontal(this IBox This, Alignment alignment)
        {
            if (alignment == Alignment.Min)
                return This.XMax.BindThreeWayTo(
                    This.Width,
                    This.XMin,
                    (width, xMin) => xMin + width,
                    (xMax, xMin) => xMax - xMin);

            if (alignment == Alignment.Max)
                return This.XMin.BindThreeWayTo(
                    This.Width,
                    This.XMax,
                    (width, xMax) => xMax - width,
                    (xMin, xMax) => xMax - xMin);

            if (alignment == Alignment.Stretch)
                return This.Width.BindThreeWayTo(
                    This.XMax,
                    This.XMin,
                    (xMax, xMin) => xMax - xMin,
                    (width, xMin) => xMin + width);

            throw new NotSupportedException($"Alignment not supported: {alignment}");
        }

        public static IDisposable BindVertical(this IBox This, Alignment alignment)
        {
            if (alignment == Alignment.Min)
                return This.YMax.BindThreeWayTo(
                    This.Height,
                    This.YMin,
                    (height, yMin) => yMin + height,
                    (yMax, yMin) => yMax - yMin);

            if (alignment == Alignment.Max)
                return This.YMin.BindThreeWayTo(
                    This.Height,
                    This.YMax,
                    (height, yMax) => yMax - height,
                    (yMin, yMax) => yMax - yMin);

            if (alignment == Alignment.Stretch)
                return This.Height.BindThreeWayTo(
                    This.YMax,
                    This.YMin,
                    (yMax, yMin) => yMax - yMin,
                    (height, yMin) => yMin + height);

            throw new NotSupportedException($"Alignment not supported: {alignment}");
        }

        public static IObservable<Rect> ToRectObservable(this IBox This) =>
            This.XMin.CombineLatest(
                This.YMin,
                This.Width,
                This.Height,
                (xMin, yMin, width, height) => new Rect(xMin, yMin, width, height));
    }
}