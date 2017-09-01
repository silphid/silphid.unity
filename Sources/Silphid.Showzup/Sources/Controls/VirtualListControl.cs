using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using Silphid.Showzup.ListLayouts;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    /// <summary>
    /// - Ensure to specify Layout property (Horizontal/VerticalListLayout).
    /// - Ensure all four anchors of Container are set to upper-left corner.
    /// - Size of Container and items will be calculated dynamically by Layout.
    /// </summary>
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

            if (Layout == null)
                throw new ArgumentNullException(nameof(Layout));
            if (Viewport == null)
                throw new ArgumentNullException(nameof(Viewport));
            
            _containerRectTransform = Container.RectTransform();
            _containerRectTransform
                .ObserveEveryValueChanged(x => x.anchoredPosition)
                .Subscribe(_ => UpdateVisibleRange())
                .AddTo(this);
        }
        
        protected override IObservable<IView> LoadViews(List<Entry> entries, Options options)
        {
            _options = options;
            _entries?.ForEach(x => x.Disposable?.Dispose());
            _entries = entries;
            _views.AddRange(Enumerable.Repeat<IView>(null, _entries.Count));
            
            UpdateContainerLayout(_entries.Count);
            UpdateVisibleRange();
                
            return Observable.Empty<IView>();
        }

        private void UpdateVisibleRange()
        {
            // Get visible range and clamp it to valid range for entries
            var newRange = Layout
                .GetVisibleIndexRange(VisibleRect)
                .IntersectionWith(new IndexRange(0, _entries.Count));
            
            // Changed?
            if (newRange.Equals(_currentRange))
                return;

            // Update range
            var oldRange = _currentRange;
            _currentRange = newRange;

            // Remove views that moved out of range 
            for (var i = oldRange.Start; i < oldRange.End; i++)
                if (!newRange.Contains(i))
                    RemoveView(i);

            // Add views that moved into range 
            for (var i = newRange.Start; i < newRange.End; i++)
                if (!oldRange.Contains(i))
                    AddView(i);
        }

        private void AddView(int index)
        {
            var entry = GetEntryWithViewInfo(index);

            // Defensive/optional code
            if (entry.View != null)
                return;
            entry.Disposable?.Dispose();

            var disposables = new CompositeDisposable();
            entry.Disposable = disposables;
            disposables.Add(
                LoadView(entry.ViewInfo.Value)
                    .Subscribe(view =>
                    {
                        // Optimization/optional
                        if (disposables.IsDisposed)
                            return;

                        entry.View = view;
                        disposables.Add(
                            Disposable.Create(() =>
                                RemoveView(view, index)));
                        UpdateViewLayout(view, index);
                        AddView(Container, view);
                    }));
        }

        private void UpdateViewLayout(IView view, int index)
        {
            var rect = Layout.GetItemRect(index);
            
            var rectTransform = view.GameObject.RectTransform();
            rectTransform.pivot = Vector2.zero;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.sizeDelta = rect.size;
            rectTransform.anchoredPosition = rect.position;
        }

        private void UpdateContainerLayout(int count)
        {
            Container.RectTransform().sizeDelta = Layout.GetContainerSize(count); 
        }

        private Entry GetEntryWithViewInfo(int index)
        {
            var entry = _entries[index];
            
            // Do not resolve view if already done in the past
            if (entry.ViewInfo != null)
                return entry;

            entry.ViewInfo = ResolveView(entry.Model, _options);
            return entry;
        }

        private void RemoveView(int index)
        {
            var entry = _entries[index];
            entry.Disposable?.Dispose();
            entry.Disposable = null;
        }

        private void RemoveView(IView view, int index)
        {
            var entry = _entries[index];
            
            // Defensive/optional code
            if (entry.View != view)
                return;
            
            RemoveView(view.GameObject);
            entry.View = null;
        }

        private Rect VisibleRect =>
            new Rect(
                -_containerRectTransform.anchoredPosition,
                Viewport.GetSize());
    }
}