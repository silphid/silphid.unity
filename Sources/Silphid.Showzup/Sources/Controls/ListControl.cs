using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Silphid.Extensions;
using Silphid.Injexit;
using UniRx;
using Rx = UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public class ListControl : PresenterControl, IListPresenter
    {
        #region Injected properties

        [Inject]
        internal IViewResolver ViewResolver { get; set; }

        [Inject]
        internal IViewLoader ViewLoader { get; set; }

        [Inject]
        internal IVariantProvider VariantProvider { get; set; }

        #endregion

        #region Config properties

        public GameObject Container;
        public string[] Variants;
        public bool AutoSelect = true;

        #endregion

        #region Public properties

        public ReadOnlyReactiveProperty<IView[]> Views { get; }

        public ReadOnlyReactiveProperty<object[]> Models => _models ?? (_models = _reactiveViews
                                                                .Select(views => views
                                                                    .Select(view => view.ViewModel?.Model)
                                                                    .ToArray())
                                                                .ToReadOnlyReactiveProperty());

        public int Count => _views.Count;
        public bool HasItems => _views.Count > 0;
        public int? LastIndex => HasItems ? _views.Count - 1 : (int?) null;
        public int? FirstIndex => HasItems ? 0 : (int?) null;
        public Comparer<IView> ViewComparer { get; set; }
        public Comparer<IViewModel> ViewModelComparer { get; set; }
        public Comparer<object> ModelComparer { get; set; }

        public void SetViewComparer<TView>(Func<TView, TView, int> comparer) where TView : IView =>
            ViewComparer = Comparer<IView>.Create((x, y) => comparer((TView) x, (TView) y));

        public void SetViewModelComparer<TViewModel>(Func<TViewModel, TViewModel, int> comparer) where TViewModel : IViewModel =>
            ViewModelComparer = Comparer<IViewModel>.Create((x, y) => comparer((TViewModel) x, (TViewModel) y));

        public void SetModelComparer<TModel>(Func<TModel, TModel, int> comparer) =>
            ModelComparer = Comparer<object>.Create((x, y) => comparer((TModel) x, (TModel) y));

        #endregion


        private readonly ReactiveProperty<bool> _isLoading = new ReactiveProperty<bool>(false);
        protected readonly List<IView> _views = new List<IView>();
        private readonly ReactiveProperty<IView[]> _reactiveViews = new ReactiveProperty<IView[]>(Array.Empty<IView>());
        private VariantSet _variantSet;
        private ReadOnlyReactiveProperty<object[]> _models;

        protected VariantSet VariantSet =>
            _variantSet ??
            (_variantSet = VariantProvider.GetVariantsNamed(Variants));

        public ListControl()
        {
            IsLoading = _isLoading.ToReadOnlyReactiveProperty();
            Views = _reactiveViews.ToReadOnlyReactiveProperty();
        }

        protected virtual void Start()
        {
            Views
                .Select(x => x.FirstOrDefault())
                .Do(x => MutableFirstView.Value = x)
                .Where(x => AutoSelect)
                .CombineLatest(IsSelected.WhereTrue(), (x, y) => x)
                .Subscribe(SelectView)
                .AddTo(this);
        }

        protected virtual void SelectView(IView view)
        {
            view?.SelectDeferred();
        }

        public IView GetViewForViewModel(object viewModel) =>
            _views.FirstOrDefault(x => x?.ViewModel == viewModel);

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
                .Do(AddView)
                .DoOnCompleted(UpdateReactiveViews);

        public void Remove(object input)
        {
            var view = _views.FirstOrDefault(x => x == input || x.ViewModel == input || x.ViewModel?.Model == input);
            if (view == null)
                return;

            RemoveView(view.GameObject);
            UpdateReactiveViews();
        }

        [Pure]
        public override IObservable<IView> Present(object input, Options options = null)
        {
            //Review this make possible to use Observable.next for populate the list 
            var observable = input as IObservable<object>;
            if (observable != null)
                return observable.SelectMany(x => Present(x, options));

            options = Options.CloneWithExtraVariants(options, VariantProvider.GetVariantsNamed(Variants));

            if (!(input is IEnumerable))
                input = new[] {input};

            return PresentInternal((IEnumerable) input, options);
        }

        [Pure]
        private IObservable<IView> PresentInternal(IEnumerable items, Options options = null)
        {
            return Observable
                .Defer(() => CleanUpAndLoadViews(items, options))
                .Do(AddView);
        }

        private IObservable<IView> CleanUpAndLoadViews(IEnumerable items, Options options)
        {
            _views.Clear();
            _reactiveViews.Value = Array.Empty<IView>();
            RemoveAllViews(Container);

            return LoadViews(items, options);
        }

        private void AddView(IView view)
        {
            var index = GetInsertionIndex(view);
            if (index.HasValue)
            {
                _views.Insert(index.Value, view);
                InsertView(Container, index.Value, view);
            }
            else
            {
                _views.Add(view);
                AddView(Container, view);
            }
        }

        private int? GetInsertionIndex(IView view)
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

        private IObservable<IView> LoadViews(IEnumerable items, Options options)
        {
            if (items == null)
                return Observable.Empty<IView>();
            _isLoading.Value = true;
            return items
                .Cast<object>()
                .Select(input => ResolveView(input, options))
                .ToObservable()
                .SelectMany(view => LoadView(view))
                .DoOnCompleted(UpdateReactiveViews);
        }

        private void UpdateReactiveViews()
        {
            _isLoading.Value = false;
            _reactiveViews.Value = _views.ToArray();
        }

        protected ViewInfo ResolveView(object input, Options options) =>
            ViewResolver.Resolve(input, options);

        protected virtual IObservable<IView> LoadView(ViewInfo viewInfo) =>
            ViewLoader.Load(viewInfo, CancellationToken.None);
    }
}