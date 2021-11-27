using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Silphid.DataTypes;
using Silphid.Extensions;
using Silphid.Injexit;
using Silphid.Requests;
using Silphid.Showzup.Recipes;
using Silphid.Showzup.Requests;
using Silphid.Showzup.Virtual.Layout;
using UniRx;
using UniRx.Completables;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Silphid.Showzup.Virtual
{
    [RequireComponent(typeof(RectTransform))]
    public class ListControl : MonoBehaviour, IListPresenter, IContainerProvider, IRequestHandler
    {
        // Priority #1:
        // TODO: Fix fixed layout
        // TODO: Fix non-virtualized mode
        // TODO: Fix items that continue loading even when viewport has moved away since a while (dispose should cancel loading).
        // TODO: Optimization: Track loaded index range to iterate only through loaded range during relayout instead of almost complete collection.
        // TODO: Test virtualization with small number of items (when they all fit within viewport)
        // TODO: OnDestroy, dispose all pending loading views / unload all views
        // TODO: *JSBR* Find why the error is not display (in function FindViewport)

        // Priority #2:
        // TODO: Drag-and-drop.
        // TODO: Paging.

        // Priority #3:
        // TODO: View pooling.

        // Priority #4:
        // TODO: Test nested list controls.
        // TODO: Reintegrate IMoveHandler handling for TV remote navigation.
        // TODO: Reintegrate IRequestHandler implementation for PresentRequest.
        // TODO: Relayout automatically when some item's preferred size changes.

        // Priority #5:
        // TODO: Animated/transitioned item adding/removal (maybe only for single-item across, aka "Stack").
        // TODO: Proper implementation of ICompletable returned by Present().
        // TODO: Optimize virtualization on scroll to avoid iterating through all RectTransforms every time (use a differential approach or throttle?)
        // TODO: Fix relayout upon item removal and load more views at the end as appropriate.
        // TODO: Unload extra views at the end upon item insertion.

        #region Private fields

        private readonly List<Entry> _entries = new List<Entry>();
        private readonly ISubject<IView> _loadedViewsSubject = new Subject<IView>();
        private readonly ISubject<IView> _unloadedViewsSubject = new Subject<IView>();
        private IReadOnlyList<object> _models;
        private IOptions _options;
        private CompositeDisposable _reactiveCollectionDisposable;
        private ICompletableSubject _completableSubject;
        private bool _isInitialized;
        private ILayoutCollection _layoutCollection;
        private ILayout _layout;
        private RectTransform _contentTransform;
        private LayoutElement _contentLayoutElement;
        private RectTransform _viewportTransform;
        private ScrollRectEx _scrollRectEx;
        private readonly ReactiveProperty<IntRange> _activeRange = new ReactiveProperty<IntRange>(IntRange.Empty);
        private RangeCache _ranges;
        private readonly ChoiceHelper _choiceHelper;

        #endregion

        #region Config properties

        public RectTransform Content;
        public string[] Variants;
        public LayoutInfo LayoutInfo = new LayoutInfo();
        public bool Virtualize;
        public bool HandlesChooseRequest;
        public bool ConsumeRequest;

        private Vector2 AvailableSize => LayoutInfo.Transformer.TransformSize(_contentTransform.rect.size);

        private Rect ViewportRect =>
            LayoutInfo.Transformer.TransformRect(_viewportTransform.GetRectRelativeTo(Content));

        #endregion

        #region Injected properties

        [Inject, UsedImplicitly] private IRecipeProvider RecipeProvider { get; set; }
        [Inject, UsedImplicitly] private IViewLoader ViewLoader { get; set; }
        [Inject, UsedImplicitly] private IVariantProvider VariantProvider { get; set; }

        #endregion

        #region Initialization

        public ListControl()
        {
            _choiceHelper = new ChoiceHelper(this);
        }

        private void EnsureInitialized()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            _layoutCollection = new LayoutCollection(_entries, LayoutInfo);
            _ranges = new RangeCache(_layoutCollection);
            _layout = new DelegatingLayout(LayoutInfo, _ranges);

            AssertDependencies();
            InitViewport();

            if (Virtualize)
                InitVirtualization();
        }

        private void AssertDependencies()
        {
            if (LayoutInfo == null)
                throw new ArgumentNullException(nameof(LayoutInfo));

            if (Content == null)
                throw new ArgumentNullException(nameof(Content));

            _contentTransform = Content.GetComponent<RectTransform>();

            _contentLayoutElement = Content.GetComponent<LayoutElement>();
            if (_contentLayoutElement == null)
                throw new InvalidOperationException("Content must have a LayoutElement component");
        }

        private void InitViewport()
        {
            var viewport = FindViewport();
            _viewportTransform = viewport.RectTransform;
            _scrollRectEx = viewport.ScrollRectEx;
        }

        public void Start()
        {
            EnsureInitialized();
        }

        private void OnDestroy()
        {
            StopObservingReactiveCollection();
        }

        #endregion

        #region IContainerProvider members

        private IContainer _container;

        public IContainer Container =>
            _container ?? (_container = this.GetParentContainer());

        #endregion

        #region IListPresenter

        public ICompletable Present(IReadOnlyList<object> models, IOptions options = null) =>
            Completable.Defer(() => PresentInternal(models, options))
                       .DoOnCompleted(() => UpdateLayout());

        public IObservable<IView> LoadedViews => _loadedViewsSubject;
        public IObservable<IView> UnloadedViews => _unloadedViewsSubject;

        #endregion

        #region Present implementation

        private ICompletable PresentInternal(IReadOnlyList<object> models, IOptions options)
        {
            EnsureInitialized();
            lock (this)
            {
                _models = models;
                _options = options.With(VariantProvider.GetVariantsNamed(Variants));

                StopObservingReactiveCollection();
                if (models is IReadOnlyReactiveCollection<object> reactiveModels)
                    ObserveReactiveCollection(reactiveModels);

                RecreateAllEntries();
                LoadInitialViews();

                _completableSubject?.OnCompleted();
                _completableSubject = new CompletableSubject();
                return _completableSubject;
            }
        }

        private void RecreateAllEntries()
        {
            _entries.ForEach(x => x.Disposable?.Dispose());
            _entries.Clear();
            foreach (var model in _models)
                _entries.Add(new Entry(model));
        }

        #endregion

        #region View loading/unloading

        private void LoadInitialViews()
        {
            if (Virtualize)
                ResetActiveRange();
            else
                foreach (var entry in _entries)
                    LoadView(entry);
        }

        private void LoadView(int index, LayoutDirection direction = LayoutDirection.Forward)
        {
            LoadView(_entries[index], direction);
        }

        private void LoadView(Entry entry, LayoutDirection direction = LayoutDirection.Forward)
        {
            lock (this)
            {
                if (entry.IsViewLoaded)
                    return;

                //Debug.Log("Load: " + _entries.IndexOf(entry));

                entry.Disposable?.Dispose();

                // var cancellation = new CancellationDisposable();
                var disposable = new SerialDisposable(); // { Disposable = cancellation };
                entry.Disposable = disposable;
                ResolveRecipe(entry)
                   .ContinueWith(recipe => LoadView(recipe, CancellationToken.None)) //cancellation.Token)
                   .Subscribe(
                        view =>
                        {
                            lock (this)
                            {
                                if (disposable.IsDisposed)
                                {
                                    //Debug.Log($"IsDisposed Destroy: {_entries.IndexOf(entry)}");
                                    view.GameObject.Destroy();
                                }
                                else
                                {
                                    entry.View = view;
                                    entry.IsLayoutValid = false;
                                    disposable.Disposable = Disposable.Create(() => UnloadViewInternal(entry));
                                    view.GameObject.SetParent(Content.gameObject);
                                    var index = _entries.IndexOf(entry);
                                    _ranges.OnItemLoaded(index);
                                    UpdateLayout(direction);
                                }
                            }

                            if (entry.View != null)
                                _loadedViewsSubject.OnNext(entry.View);
                        });
            }
        }

        private IObservable<Recipe> ResolveRecipe(Entry entry) =>
            entry.Recipe == null
                ? RecipeProvider.GetRecipe(entry.Model, _options)
                                .Do(x => entry.Recipe = x)
                : Observable.Return(entry.Recipe.Value);

        private IObservable<IView> LoadView(Recipe recipe, CancellationToken cancellationToken) =>
            ViewLoader.Load(GetInstantiationContainer(), recipe, Container, cancellationToken);

        private void UnloadView(int index)
        {
            //Debug.Log("Unload: " + index);
            _entries[index]
               .Disposable?.Dispose();
        }

        private void UnloadViewInternal(Entry entry)
        {
            IView unloadedView;

            lock (this)
            {
                if (!entry.IsViewLoaded)
                    return;

                unloadedView = entry.View;
                entry.View = null;
                entry.IsLayoutValid = false;
                unloadedView.GameObject.Destroy();
                var index = _entries.IndexOf(entry);
                _ranges.OnItemUnloaded(index);
            }

            _unloadedViewsSubject.OnNext(unloadedView);
        }

        #endregion

        #region ReactiveCollection

        private void StopObservingReactiveCollection()
        {
            _reactiveCollectionDisposable?.Dispose();
            _reactiveCollectionDisposable = null;
        }

        private void ObserveReactiveCollection(IReadOnlyReactiveCollection<object> models)
        {
            _reactiveCollectionDisposable = new CompositeDisposable(
                models.ObserveAdd()
                      .Subscribe(x => OnAdd(x.Index, x.Value)),
                models.ObserveRemove()
                      .Subscribe(x => OnRemove(x.Index, x.Value)),
                models.ObserveMove()
                      .Subscribe(x => OnMove(x.OldIndex, x.NewIndex)),
                models.ObserveReplace()
                      .Subscribe(x => OnReplace(x.Index, x.NewValue)),
                models.ObserveReset()
                      .Subscribe(x => OnReset()));
        }

        private void OnAdd(int index, object model)
        {
            lock (this)
            {
                var entry = new Entry(model);
                _entries.Insert(index, entry);

                // TODO: Update RangeCache
                if (_activeRange.Value.Contains(index))
                    LoadView(entry);
            }
        }

        private void OnRemove(int index, object model)
        {
            lock (this)
            {
                _entries[index]
                   .Disposable?.Dispose();
                _entries.RemoveAt(index);

                UpdateLayout();

                // TODO: Update RangeCache
                // TODO: Update layout only if was within visible range
            }
        }

        private void OnMove(int oldIndex, int newIndex)
        {
            lock (this)
            {
                var entry = _entries[oldIndex];
                _entries.RemoveAt(oldIndex);
                _entries.Insert(newIndex, entry);
                UpdateLayout();

                // TODO: Update RangeCache
                // TODO: Update layout if was or becomes within visible range
            }
        }

        private void OnReplace(int index, object model)
        {
            lock (this)
            {
                _entries[index]
                   .Disposable?.Dispose();
                var entry = new Entry(model);
                _entries[index] = entry;
                LoadView(entry);

                // TODO: Update RangeCache
                // TODO: Load view only if within visible range
            }
        }

        private void OnReset()
        {
            lock (this)
            {
                RecreateAllEntries();

                // TODO: Update RangeCache
                // TODO: Load all views (but only within visible range ???)

                foreach (var entry in _entries.Where((_, index) => _activeRange.Value.Contains(index)))
                    LoadView(entry);
            }
        }

        #endregion

        #region Instantiation container

        private Transform _instantiationContainer;

        private Transform GetInstantiationContainer()
        {
            if (_instantiationContainer == null)
            {
                var obj = new GameObject("InstantiationContainer");
                obj.SetActive(false);
                _instantiationContainer = obj.transform;
                _instantiationContainer.transform.parent = transform;
            }

            return _instantiationContainer;
        }

        #endregion

        #region Virtualization

        private void InitVirtualization()
        {
            _scrollRectEx.content.ObserveEveryValueChanged(x => x.anchoredPosition)
                         .AsUnitObservable()
                         .Merge(Content.OnRectTransformDimensionsChangeAsObservable())
                         .Subscribe(_ => UpdateActiveRange())
                         .AddTo(this);

            _activeRange.PairWithPrevious()
                        .Subscribe(x => OnActiveRangeChanged(x.Item1, x.Item2))
                        .AddTo(this);
        }

        private void ResetActiveRange()
        {
            _activeRange.Value = IntRange.Empty;
            _ranges.Invalidate();
            UpdateActiveRange();
        }

        private void UpdateActiveRange()
        {
            _activeRange.Value = GetActiveRange();
        }

        private IntRange GetActiveRange() =>
            _layout.GetActiveRange(_layoutCollection, ViewportRect, AvailableSize);

        private void OnActiveRangeChanged(IntRange oldRange, IntRange newRange)
        {
            //Debug.Log($"Range: {newRange}");
            var combinedRange = oldRange.Union(newRange);

            foreach (var index in combinedRange)
            {
                bool isInOld = oldRange.Contains(index);
                bool isInNew = newRange.Contains(index);

                if (isInOld && !isInNew)
                    UnloadView(index);
                else if (!isInOld && isInNew)
                {
                    var direction = oldRange.IsEmpty || index >= oldRange.Start
                                        ? LayoutDirection.Forward
                                        : LayoutDirection.Backward;

                    LoadView(index, direction);
                }
            }
        }

        private Viewport FindViewport()
        {
            var viewport = this.SelfAndAncestors<Viewport>()
                               .FirstOrDefault();

            //TODO find why error trigger
            if (viewport == null)
                throw new InvalidOperationException(
                    "Virtualization requires a Viewport component on ListControl or some ancestor");

            if (viewport.RectTransform == null)
                throw new InvalidOperationException(
                    "Virtualization requires that RectTransform property of Viewport component be set");

            if (viewport.ScrollRectEx == null)
                throw new InvalidOperationException(
                    "Virtualization requires that ScrollRectEx property of Viewport component be set");

            return viewport;
        }

        #endregion

        #region Layout

        private void OnRectTransformDimensionsChange()
        {
            EnsureInitialized();
            UpdateLayout();
        }

        public void UpdateLayout(LayoutDirection direction = LayoutDirection.Forward)
        {
            var (contentSize, adjustment) = _layout.Perform(direction, _layoutCollection, ViewportRect, AvailableSize);

            SetContentSize(contentSize);
            AdjustScrollRect(adjustment);
        }

        private void SetContentSize(float size)
        {
            if (LayoutInfo.Orientation == Orientation.Vertical)
                _contentLayoutElement.minHeight = size;
            else
                _contentLayoutElement.minWidth = size;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRectEx.content);
        }

        private void AdjustScrollRect(float adjustment)
        {
            _scrollRectEx.Adjust(LayoutInfo.Transformer.InverseTransformSize(new Vector2(0, adjustment)));
        }

        #endregion

        public bool Handle(IRequest request) => _choiceHelper.Handle(
            request,
            _entries,
            HandlesChooseRequest,
            ConsumeRequest);
    }
}