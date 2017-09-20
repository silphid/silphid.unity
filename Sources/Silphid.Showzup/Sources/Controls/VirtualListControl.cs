using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Silphid.Extensions;
using Silphid.Injexit;
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
        private static readonly ILog Log = LogManager.GetLogger(typeof(VirtualListControl));
        
        private Options _options;
        private List<Entry> _entries = new List<Entry>();
        private IndexRange _currentRange = IndexRange.Empty;
        private RectTransform _containerRectTransform;
        private bool _isStarted;
        private Action _queuedLoadViews;
        
        public ListLayout Layout;
        public RectTransform Viewport;
        public RectTransform ScrollingContent;
        public int ExtraMarginItems = 3;
        
        public override ReadOnlyReactiveProperty<bool> IsLoading { get { throw new NotSupportedException(); } }

        protected override void Start()
        {
            base.Start();

            if (Layout == null)
                throw new ArgumentNullException(nameof(Layout));
            if (Viewport == null)
                throw new ArgumentNullException(nameof(Viewport));
            
            _containerRectTransform = ScrollingContent != null ? ScrollingContent : Container.RectTransform();
            
            if (_containerRectTransform == null)
                throw new ArgumentException("Container must have a RectTransform component.");
            
            _containerRectTransform
                .ObserveEveryValueChanged(x => x.anchoredPosition)
                .Subscribe(_ => UpdateVisibleRange())
                .AddTo(this);

            lock (this)
            {
                _isStarted = true;
                _queuedLoadViews?.Invoke();
            }
        }

        public override IObservable<IView> PresentInternal(object input, Options options = null)
        {
            _currentRange = IndexRange.Empty;
            return base.PresentInternal(input, options);
        }

        protected override IObservable<IView> LoadViews(List<Entry> entries, Options options)
        {
            if (!EnqueueLoadViewsIfNotYetStarted(entries, options))
            {
                _options = options;
                _entries?.ForEach(x => x.Disposable?.Dispose());
                _entries = entries;
                _views.AddRange(Enumerable.Repeat<IView>(null, _entries.Count));
            
                UpdateContainerLayout(_entries.Count);
                UpdateVisibleRange();
            }
                
            return Observable.Empty<IView>();
        }

        private bool EnqueueLoadViewsIfNotYetStarted(List<Entry> entries, Options options)
        {
            lock (this)
            {
                if (_isStarted)
                    return false;

                _queuedLoadViews = () => LoadViews(entries, options).Subscribe();
                return true;
            }
        }

        private void UpdateVisibleRange()
        {
            // Get visible range and clamp it to valid range for entries
            var newRange = Layout
                .GetVisibleIndexRange(VisibleRect)
                .ExpandStartAndEndBy(ExtraMarginItems)
                .IntersectionWith(new IndexRange(0, _entries.Count));
            
            // Changed?
            if (newRange.Equals(_currentRange))
                return;
            
            // Update range
            Log.Debug($"VirtualListControl - Visible range: {newRange}");
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
            Log.Debug($"VirtualListControl - Adding view {index}");
            
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
            var rect = Layout.GetItemRect(index, Viewport.GetSize());
            
            var rectTransform = view.GameObject.RectTransform();
            rectTransform.pivot = Vector2.up;
            rectTransform.anchorMin = Vector2.up;
            rectTransform.anchorMax = Vector2.up;
            rectTransform.anchoredPosition = rect.position;
            rectTransform.sizeDelta = rect.size;
        }

        private void UpdateContainerLayout(int count)
        {
            _containerRectTransform.pivot = Vector2.up;
            _containerRectTransform.anchorMin = Vector2.up;
            _containerRectTransform.anchorMax = Vector2.up;
            _containerRectTransform.sizeDelta = Layout.GetContainerSize(count, Viewport.GetSize()); 
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
            Log.Debug($"VirtualListControl - Removing view {index}");
            if(_entries.Count<=index) return;
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
                _containerRectTransform.anchoredPosition,
                Viewport.GetSize());
    }
}