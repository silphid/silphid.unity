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
    public class ListControl : Control, IListPresenter
    {
        #region Injected properties

        [Inject] internal IViewResolver ViewResolver { get; set; }
        [Inject] internal IViewLoader ViewLoader { get; set; }
        [Inject] internal IVariantProvider VariantProvider { get; set; }

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

        #endregion

        protected readonly List<IView> _views = new List<IView>();
        private readonly ReactiveProperty<IView[]> _reactiveViews = new ReactiveProperty<IView[]>(Array.Empty<IView>());
        private VariantSet _variantSet;
        private ReadOnlyReactiveProperty<object[]> _models;

        protected VariantSet VariantSet =>
            _variantSet ??
            (_variantSet = VariantProvider.GetVariantsNamed(Variants));

        public ListControl()
        {
            Views = _reactiveViews.ToReadOnlyReactiveProperty();
        }

        protected virtual void Start()
        {
            if (AutoSelect)
                Views
                    .CombineLatest(IsSelected.WhereTrue(), (x, y) => x)
                    .Subscribe(x => SelectView(x.FirstOrDefault()));
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

        public bool CanPresent(object input, Options options = null)
        {
            var target = options?.Target;
            return target == null || VariantSet.Contains(target);
        }

        [Pure]
        public virtual IObservable<IView> Present(object input, Options options = null)
        {
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
            _views.Add(view);

            AddView(Container, view);
        }

        private IObservable<IView> LoadViews(IEnumerable items, Options options)
        {
            if (items == null)
                return Observable.Empty<IView>();

            return items
                .Cast<object>()
                .Select(input => ResolveView(input, options))
                .ToObservable()
                .SelectMany(view => LoadView(view))
                .DoOnCompleted(() => _reactiveViews.Value = _views.ToArray());
        }

        protected ViewInfo ResolveView(object input, Options options) =>
            ViewResolver.Resolve(input, options);

        protected virtual IObservable<IView> LoadView(ViewInfo viewInfo) =>
            ViewLoader.Load(viewInfo, CancellationToken.None);
    }
}