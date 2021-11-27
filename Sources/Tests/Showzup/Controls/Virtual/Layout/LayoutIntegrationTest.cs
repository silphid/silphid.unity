using Silphid.Extensions;
using Silphid.Showzup.Virtual;
using Silphid.Showzup.Virtual.Layout;
using UnityEngine;

namespace Silphid.Showzup.Test.Controls.Virtual.Layout
{
    public class LayoutIntegrationTest : MonoBehaviour
    {
        private class LayoutCollection : ILayoutCollection
        {
            private readonly RectTransform[] _transforms;

            public LayoutCollection(RectTransform[] transforms)
            {
                _transforms = transforms;
            }

            public int Count => _transforms.Length;
            public bool IsLoaded(int index) => true;
            public bool IsLayouted(int index) => false;

            public Vector2 GetPreferredSize(int index) =>
                _transforms[index]
                   .GetPreferredSize();

            public Rect GetRect(int index) =>
                _transforms[index]
                   .rect;

            public void SetRect(int index, Rect rect) =>
                _transforms[index]
                   .SetLayoutRect(rect);
        }

        public RectTransform Container;
        public RectTransform Bottom;
        public RectTransform[] Transforms;
        public LayoutInfo LayoutInfo = new LayoutInfo();

        private void Update()
        {
            var rect = Container.rect;
            var collection = new LayoutCollection(Transforms);
            var ranges = new RangeCache(collection);
            var layout = new DelegatingLayout(LayoutInfo, ranges);

            var (requiredSizeAlong, _) = layout.Perform(
                LayoutDirection.Forward,
                collection,
                new Rect(Vector2.zero, rect.size),
                rect.size);

            Bottom.SetLayoutRect(new Rect(0, requiredSizeAlong, Container.rect.width, 2));
        }
    }
}