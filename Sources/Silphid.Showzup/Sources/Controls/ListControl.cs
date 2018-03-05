using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Silphid.Extensions;
using Silphid.Injexit;
using Silphid.Requests;
using Silphid.Showzup.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Silphid.Showzup
{
    public class ListControl : PresenterControl, IListPresenter, IMoveHandler, IRequestHandler
    {
        #region Entry private class

        protected class Entry
        {
            public int Index { get; }
            public object Model { get; }
            public ViewInfo? ViewInfo { get; set; }
            public IView View { get; set; }
            public IDisposable Disposable { get; set; }

            public Entry(int index, object model)
            {
                Index = index;
                Model = model;
            }

            public Entry(int index, IView view)
            {
                Index = index;
                View = view;
                Model = view.ViewModel?.Model ?? view.ViewModel;
            }
        }

        #endregion

        #region Injected properties

        [Inject, UsedImplicitly] internal IViewResolver ViewResolver { get; set; }
        [Inject, UsedImplicitly] internal IViewLoader ViewLoader { get; set; }
        [Inject, UsedImplicitly] internal IVariantProvider VariantProvider { get; set; }

        #endregion

        #region Config properties

        public GameObject Container;
        public string[] Variants;
        public Comparer<IView> ViewComparer { get; set; }
        public Comparer<IViewModel> ViewModelComparer { get; set; }
        public Comparer<object> ModelComparer { get; set; }
        public NavigationOrientation Orientation;
        public bool WrapAround;
        public int RowsOrColumns = 1;
        public bool AutoSelectFirst = true;
        public bool HandlesSelectRequest;

        #endregion

        #region Public properties

        public ReadOnlyReactiveProperty<ReadOnlyCollection<IView>> Views { get; }
        public ReadOnlyReactiveProperty<ReadOnlyCollection<object>> Models { get; }
        public virtual int Count => _views.Count;
        public bool HasItems => Count > 0;
        public int? LastIndex => HasItems ? Count - 1 : (int?) null;
        public int? FirstIndex => HasItems ? 0 : (int?) null;

        #endregion
        
        #region Protected/private fields/properties

        private SelectionHelper _selectionHelper;
        protected readonly List<IView> _views = new List<IView>();
        private readonly ReactiveProperty<ReadOnlyCollection<IView>> _reactiveViews =
            new ReactiveProperty<ReadOnlyCollection<IView>>(new ReadOnlyCollection<IView>(Array.Empty<IView>()));

        private VariantSet _variantSet;

        private readonly ReactiveProperty<List<object>> _models = new ReactiveProperty<List<object>>(new List<object>())
            ;

        protected VariantSet VariantSet =>
            _variantSet ??
            (_variantSet = VariantProvider.GetVariantsNamed(Variants));

        #endregion

        #region Constructor

        public ListControl()
        {
            Views = _reactiveViews.ToReadOnlyReactiveProperty();
            Models = _models
                .Select(x => new ReadOnlyCollection<object>(x))
                .ToReadOnlyReactiveProperty();
        }

        private void Awake()
        {
            _selectionHelper = new SelectionHelper(this);
        }

        #endregion

        #region IPresenter members

        [Pure]
        protected override IObservable<IView> PresentView(object input, Options options = null)
        {
            MutableState.Value = PresenterState.Loading;
            
            // If input is observable, resolve it first
            var observable = input as IObservable<object>;
            if (observable != null)
                return observable.ContinueWith(x => PresentView(x, options));
            
            options = options.With(VariantProvider.GetVariantsNamed(Variants));

            return Observable.Defer(() =>
                PresentInternal(input, options)
                    .DoOnCompleted(() =>
                    {
                        if (AutoSelectFirst)
                            _selectionHelper.SelectFirst();
                    }));
        }

        #endregion

        #region Selection
        
        public void OnMove(AxisEventData eventData) => _selectionHelper.OnMove(eventData);
        
        public bool Handle(IRequest request) => _selectionHelper.Handle(request);

        public override GameObject ForwardSelection() => _selectionHelper.SelectedView.Value?.GameObject;

        public IReadOnlyReactiveProperty<IView> SelectedView => _selectionHelper.SelectedView;

        public ReactiveProperty<int?> SelectedIndex => _selectionHelper.SelectedIndex;

        public IReactiveProperty<object> SelectedModel => _selectionHelper.SelectedModel;

        public void SelectView(IView view)
        {
            _selectionHelper.SelectView(view);
        }

        public void SelectView<TView>(Func<TView, bool> predicate) where TView : IView
        {
            _selectionHelper.SelectView(predicate);
        }

        public void SelectViewModel<TViewModel>(TViewModel viewModel) where TViewModel : IViewModel
        {
            _selectionHelper.SelectViewModel(viewModel);
        }

        public void SelectViewModel<TViewModel>(Func<TViewModel, bool> predicate) where TViewModel : IViewModel
        {
            _selectionHelper.SelectViewModel(predicate);
        }

        public void SelectModel<TModel>(TModel model)
        {
            _selectionHelper.SelectModel(model);
        }

        public void SelectModel<TModel>(Func<TModel, bool> predicate)
        {
            _selectionHelper.SelectModel(predicate);
        }

        public bool SelectIndex(int index)
        {
            return _selectionHelper.SelectIndex(index);
        }

        public bool SelectFirst()
        {
            return _selectionHelper.SelectFirst();
        }

        public bool SelectLast()
        {
            return _selectionHelper.SelectLast();
        }

        public void SelectNone()
        {
            _selectionHelper.SelectNone();
        }

        public bool SelectPrevious()
        {
            return _selectionHelper.SelectPrevious();
        }

        public bool SelectNext()
        {
            return _selectionHelper.SelectNext();
        }

        #endregion
        
        #region Public methods

        public void SetViewComparer<TView>(Func<TView, TView, int> comparer) where TView : IView =>
            ViewComparer = Comparer<IView>.Create((x, y) => comparer((TView) x, (TView) y));

        public void SetViewModelComparer<TViewModel>(Func<TViewModel, TViewModel, int> comparer)
            where TViewModel : IViewModel =>
            ViewModelComparer = Comparer<IViewModel>.Create((x, y) => comparer((TViewModel) x, (TViewModel) y));

        public void SetModelComparer<TModel>(Func<TModel, TModel, int> comparer) =>
            ModelComparer = Comparer<object>.Create((x, y) => comparer((TModel) x, (TModel) y));

        public IView GetViewForViewModel(object viewModel) =>
            _views?.FirstOrDefault(x => x?.ViewModel == viewModel);

        public int? IndexOfView(IView view)
        {
            if (view == null)
                return null;

            var index = _views.IndexOf(view);
            if (index == -1)
                return null;

            return index;
        }

        public IView GetViewAtIndex(int? index) =>
            index.HasValue ? _views[index.Value] : null;

        [Pure]
        public IObservable<IView> Add(object input, Options options = null) =>
            LoadView(ResolveView(input, options))
                .Do(view =>
                {
                    AddView(_views.Count, view);
                    UpdateReactiveViews();
                });

        public void Remove(object input)
        {
            var view = _views.FirstOrDefault(x => x == input || x.ViewModel == input || x.ViewModel?.Model == input);
            if (view == null)
                return;

            _views.Remove(view);
            RemoveView(view.GameObject);
            UpdateReactiveViews();
        }

        protected override void RemoveAllViews(GameObject container, GameObject except = null)
        {
            base.RemoveAllViews(container, except);

            _selectionHelper.SelectNone();
        }

        #endregion

        #region Private methods

        protected virtual IObservable<IView> PresentInternal(object input, Options options)
        {            
            var models = (input as List<object>)?.ToList() ?? 
                         (input as IEnumerable)?.Cast<object>().ToList() ??
                         input?.ToSingleItemList() ??
                         new List<object>();

            _models.Value = models;
            RemoveViews(Container, _views);
            _views.Clear();
            _reactiveViews.Value = new ReadOnlyCollection<IView>(Array.Empty<IView>());

            var entries = models
                .Select((x, i) => new Entry(i, x))
                .ToList();

            return LoadViews(entries, options);
        }

        protected virtual IObservable<IView> LoadViews(List<Entry> entries, Options options) =>
            LoadAllViews(entries, options)
                .Do(x =>
                {
                    MutableState.Value = PresenterState.Presenting;
                    AddView(x.Index, x.View);
                })
                .Select(x => x.View);

        private IObservable<Entry> LoadAllViews(List<Entry> entries, Options options)
        {
            return entries
                .Do(entry => entry.ViewInfo = ResolveView(entry.Model, options))
                .ToObservable()
                .SelectMany(entry => LoadView(entry.ViewInfo.Value)
                    .Do(view => entry.View = view)
                    .Select(_ => entry))
                .DoOnCompleted(() =>
                {
                    MutableState.Value = PresenterState.Ready;
                    UpdateReactiveViews();
                });
        }

        protected void UpdateReactiveViews() =>
            _reactiveViews.Value = _views.AsReadOnly();

        private int? GetSortedIndex(IView view)
        {
            if (_views.Count == 0)
                return null;

            if (ViewComparer != null)
            {
                for (var i = 0; i < _views.Count; i++)
                    if (ViewComparer.Compare(view, _views[i]) < 0)
                        return i;

                return null;
            }

            if (ViewModelComparer != null)
            {
                for (var i = 0; i < _views.Count; i++)
                    if (ViewModelComparer.Compare(view.ViewModel, _views[i].ViewModel) < 0)
                        return i;

                return null;
            }

            if (ModelComparer != null)
            {
                for (var i = 0; i < _views.Count; i++)
                    if (ModelComparer.Compare(view.ViewModel?.Model, _views[i].ViewModel?.Model) < 0)
                        return i;
            }

            return null;
        }

        #endregion

        #region Protected/virtual methods

        protected virtual void Start()
        {
            Views
                .Select(x => x.FirstOrDefault())
                .Subscribe(x =>
                {
                    if (IsSelfOrDescendantSelected.Value)
                        x?.Select();
                })
                .AddTo(this);
        }

        protected virtual void AddView(int index, IView view)
        {
            var sortedIndex = GetSortedIndex(view);
            if (sortedIndex != null)
            {
                InsertView(Container, sortedIndex.Value, view);
                _views.Insert(sortedIndex.Value, view);
            }
            else
            {
                AddView(Container, view);
                _views.Add(view);
            }
        }

        protected ViewInfo ResolveView(object input, Options options) =>
            ViewResolver.Resolve(input, options);

        protected virtual IObservable<IView> LoadView(ViewInfo viewInfo) =>
            ViewLoader.Load(GetInstantiationContainer(), viewInfo, CancellationToken.None);

        #endregion
    }
}