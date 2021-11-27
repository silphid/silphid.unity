using System.Linq;
using NUnit.Framework;
using Silphid.Showzup.Virtual;
using Silphid.Showzup.Virtual.Layout;
using Silphid.Tests;
using UnityEngine;

namespace Silphid.Showzup.Test.Controls.Virtual.Layout
{
    [TestFixture]
    public class LayoutTest
    {
        private class LayoutCollection : ILayoutCollection
        {
            private readonly Rect[] _initialRects;
            private readonly Rect[] _layoutedRects;
            private readonly bool _isLoaded;

            public LayoutCollection(Rect[] initialRects, Rect[] layoutedRects, bool isLoaded)
            {
                _initialRects = initialRects;
                _layoutedRects = layoutedRects;
                _isLoaded = isLoaded;
            }

            public int Count => _initialRects.Length;
            public bool IsLoaded(int index) => _isLoaded;
            public bool IsLayouted(int index) => false;

            public Vector2 GetPreferredSize(int index) => _initialRects[index]
               .size;

            public Rect GetRect(int index) => _initialRects[index];
            public void SetRect(int index, Rect rect) => _layoutedRects[index] = rect;
        }

        private const int CountAcross = 3;
        private readonly Vector2 MarginMin = new Vector2(123, 133);
        private readonly Vector2 MarginMax = new Vector2(125, 135);
        private readonly Vector2 AvailableSize = new Vector2(1000, 2000);
        private readonly Rect ViewportRect = new Rect(0, 0, 1000, 2000);
        private readonly Vector2 Size = new Vector2(251, 253);
        private readonly Vector2 SizeA = new Vector2(201, 203);
        private readonly Vector2 SizeB = new Vector2(221, 223);
        private readonly Vector2 SizeC = new Vector2(400, 300);
        private readonly Vector2[] Sizes;
        private readonly Vector2 Spacing = new Vector2(26, 29);
        private readonly float[] InitialX = { 100, 400, 700 };
        private readonly float[] InitialY = { 150, 500, 850 };

        private readonly Rect[] _initialRects;
        private Rect[] _layoutedRects;
        private RangeCache _ranges;
        private DelegatingLayout _layout;

        public LayoutTest()
        {
            Sizes = new[] { SizeA, SizeB, SizeC, SizeB, SizeC, SizeA, SizeC, SizeA };

            _initialRects = new Rect[8];

            int i = 0;
            for (int y = 0; y < 3; y++)
            for (int x = 0; x < 3 && i < 8; x++, i++)
                _initialRects[i] = new Rect(
                    InitialX[x],
                    InitialY[y],
                    Sizes[i]
                       .x,
                    Sizes[i]
                       .y);
        }

        private void SetUpVertical(LayoutCollection collection,
                                   SizingAlong sizingAlong,
                                   SizingAcross sizingAcross,
                                   Alignment alignmentX,
                                   Alignment alignmentY)
        {
            _ranges = new RangeCache(collection);
            _layout = new DelegatingLayout(
                new LayoutInfo
                {
                    Orientation = Orientation.Vertical,
                    AlignmentAlong = alignmentY,
                    AlignmentAcross = alignmentX,
                    Size = Size,
                    Spacing = Spacing,
                    CountAcross = CountAcross,
                    MinMargin = MarginMin,
                    MaxMargin = MarginMax,
                    SizingAlong = sizingAlong,
                    SizingAcross = sizingAcross
                },
                _ranges);
        }

        [SetUp]
        public void SetUp()
        {
            _layoutedRects = new Rect[_initialRects.Length];
        }

        [Test]
        public void TestVerticalFixedStretch()
        {
            var collection = new LayoutCollection(_initialRects, _layoutedRects, true);
            SetUpVertical(collection, SizingAlong.Fixed, SizingAcross.FixedSize, Alignment.Stretch, Alignment.Stretch);

            var (requiredSizeAlong, _) = _layout.Perform(
                LayoutDirection.Forward,
                collection,
                ViewportRect,
                AvailableSize);

            var x0 = MarginMin.x;
            var x1 = MarginMin.x + (AvailableSize.x - (MarginMin.x + MarginMax.x + Size.x)) / 2;
            var x2 = AvailableSize.x - MarginMax.x - Size.x;

            var y0 = MarginMin.y;
            var y1 = MarginMin.y + Size.y + Spacing.y;
            var y2 = MarginMin.y + (Size.y + Spacing.y) * 2;

            _layoutedRects[0]
               .Is(x0, y0, Size);
            _layoutedRects[1]
               .Is(x1, y0, Size);
            _layoutedRects[2]
               .Is(x2, y0, Size);
            _layoutedRects[3]
               .Is(x0, y1, Size);
            _layoutedRects[4]
               .Is(x1, y1, Size);
            _layoutedRects[5]
               .Is(x2, y1, Size);
            _layoutedRects[6]
               .Is(x0, y2, Size);
            _layoutedRects[7]
               .Is(x1, y2, Size);

            requiredSizeAlong.Is(y2 + Size.y + MarginMax.y);
        }

        [Test]
        public void TestNoItemsReady()
        {
            var collection = new LayoutCollection(_initialRects, _layoutedRects, false);
            SetUpVertical(collection, SizingAlong.Fixed, SizingAcross.FixedSize, Alignment.Stretch, Alignment.Stretch);

            var (requiredSizeAlong, _) = _layout.Perform(
                LayoutDirection.Forward,
                collection,
                ViewportRect,
                AvailableSize);

            requiredSizeAlong.Is(MarginMin.y + MarginMax.y);
        }

        private float GetMaxHeight(int start, int end) =>
            _initialRects.Skip(start)
                         .Take(end - start)
                         .Max(x => x.size.y);

        [Test]
        public void TestVerticalVariable()
        {
            var collection = new LayoutCollection(_initialRects, _layoutedRects, true);
            SetUpVertical(collection, SizingAlong.Variable, SizingAcross.FixedSize, Alignment.Stretch, Alignment.Min);

            var (requiredSizeAlong, _) = _layout.Perform(
                LayoutDirection.Forward,
                collection,
                ViewportRect,
                AvailableSize);

            var x0 = MarginMin.x;
            var x1 = MarginMin.x + (AvailableSize.x - (MarginMin.x + MarginMax.x + Size.x)) / 2;
            var x2 = AvailableSize.x - MarginMax.x - Size.x;

            var sizeY0 = GetMaxHeight(0, 3);
            var sizeY1 = GetMaxHeight(3, 6);
            var sizeY2 = GetMaxHeight(6, 8);

            var y0 = MarginMin.y;
            var y1 = MarginMin.y + sizeY0 + Spacing.y;
            var y2 = MarginMin.y + sizeY0 + Spacing.y + sizeY1 + Spacing.y;

            _layoutedRects[0]
               .Is(
                    x0,
                    y0,
                    Size.x,
                    Sizes[0]
                       .y);
            _layoutedRects[1]
               .Is(
                    x1,
                    y0,
                    Size.x,
                    Sizes[1]
                       .y);
            _layoutedRects[2]
               .Is(
                    x2,
                    y0,
                    Size.x,
                    Sizes[2]
                       .y);
            _layoutedRects[3]
               .Is(
                    x0,
                    y1,
                    Size.x,
                    Sizes[3]
                       .y);
            _layoutedRects[4]
               .Is(
                    x1,
                    y1,
                    Size.x,
                    Sizes[4]
                       .y);
            _layoutedRects[5]
               .Is(
                    x2,
                    y1,
                    Size.x,
                    Sizes[5]
                       .y);
            _layoutedRects[6]
               .Is(
                    x0,
                    y2,
                    Size.x,
                    Sizes[6]
                       .y);
            _layoutedRects[7]
               .Is(
                    x1,
                    y2,
                    Size.x,
                    Sizes[7]
                       .y);

            requiredSizeAlong.Is(y2 + sizeY2 + MarginMax.y);
        }
    }
}