using System.Collections.Generic;
using Silphid.Extensions;
using Silphid.Showzup.Virtual.Layout;
using UnityEngine;

namespace Silphid.Showzup.Virtual
{
    internal class LayoutCollection : ILayoutCollection
    {
        private readonly IList<Entry> _entries;
        private readonly ILayoutInfo _layoutInfo;

        public LayoutCollection(IList<Entry> entries, ILayoutInfo layoutInfo)
        {
            _entries = entries;
            _layoutInfo = layoutInfo;
        }

        public int Count => _entries.Count;

        public bool IsLoaded(int index) =>
            _entries[index]
               .IsViewLoaded;

        public bool IsLayouted(int index) =>
            _entries[index]
               .IsLayoutValid;

        public Vector2 GetPreferredSize(int index) =>
            _layoutInfo.Transformer.TransformSize(
                _entries[index]
                   .RectTransform.GetPreferredSize());

        public Rect GetRect(int index) =>
            _layoutInfo.Transformer.TransformRect(
                _entries[index]
                   .RectTransform.GetLayoutRect());

        public void SetRect(int index, Rect rect)
        {
            var entry = _entries[index];
            entry.RectTransform.SetLayoutRect(_layoutInfo.Transformer.InverseTransformRect(rect));
            entry.IsLayoutValid = true;
        }
    }
}