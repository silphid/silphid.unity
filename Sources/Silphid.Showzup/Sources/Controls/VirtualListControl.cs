using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using Silphid.Showzup.ListLayouts;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public class VirtualListControl : ListControl
    {
        private Options _options;
        private List<Entry> _entries;
        private IndexRange _currentRange = IndexRange.Empty;
        private RectTransform _containerRectTransform;
        
        public ListLayout Layout;
        public RectTransform Viewport;

        protected override void Start()
        {
            base.Start();
            _containerRectTransform = Container.GetComponent<RectTransform>();
        }
        
        protected override IObservable<IView> LoadViews(List<Entry> entries, Options options)
        {
            _options = options;
            _entries = entries;
            _views.AddRange(Enumerable.Repeat<IView>(null, _entries.Count));
            
            var newRange = Layout.GetVisibleIndexRange(VisibleRect);
            SetRange(newRange);

            return Observable.Empty<IView>();
        }

        private void SetRange(IndexRange newRange)
        {
            // Clamp to actual size
            newRange = newRange.IntersectionWith(new IndexRange(0, _entries.Count));
            
            // Changed?
            if (newRange.Equals(_currentRange))
                return;

            // Remove views that moved out of range 
            for (var i = _currentRange.Start; i < _currentRange.End; i++)
                if (!newRange.Contains(i))
                    RemoveView(i);

            // Add views that moved into range 
            for (var i = newRange.Start; i < newRange.End; i++)
                if (!_currentRange.Contains(i))
                    AddView(i);

            _currentRange = newRange;
        }

        private void AddView(int index)
        {
            // TODO
        }

        private void RemoveView(int index)
        {
            var entry = _entries[index];
            entry.Disposable?.Dispose();
            entry.Disposable = null;
        }

        private Rect VisibleRect =>
            new Rect(
                -_containerRectTransform.anchoredPosition,
                Viewport.GetSize());
    }
}