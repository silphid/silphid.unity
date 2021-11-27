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
using Silphid.Showzup.Recipes;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Silphid.Showzup
{
    public class ListControl : PresenterControl, IListPresenter, IMoveHandler, IRequestHandler
    {
        #region Entry private class

        protected class Entry
        {
            public int Index { get; }
            public object Model { get; }
            public Recipe? Recipe { get; set; }
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

        [Inject, UsedImplicitly] internal IViewLoader ViewLoader { get; set; }

        #endregion

        #region Config properties

        [FormerlySerializedAs("Container")] public GameObject Content;
        public string[] Variants;
        public Comparer<IView> ViewComparer { get; set; }
        public Comparer<IViewModel> ViewModelComparer { get; set; }
        public Comparer<object> ModelComparer { get; set; }
        public NavigationOrientation Orientation;
        public bool WrapAround;
        public int RowsOrColumns = 1;
        public bool AutoChooseFirst = true;
        public bool HandlesChooseRequest;
        [Tooltip("Skip inactive GameObjects while navigating")]
        public bool ExcludeInactiveGameObjects;
        [Tooltip("Skip GameObjects with null SelectableContent while navigating")]
        public bool ExcludeUnselectableGameObjects;
		public bool AddBeforeExistingObject;
        #endregion

        #region Public properties

        public ReadOnlyReactiveProperty<ReadOnlyCollection<IView>> Views { get; }
        public ReadOnlyReactiveProperty<ReadOnlyCollection<object>> Models { get; }
        public virtual int Count => _views.Count;

        public int RowOrColumnCount => RowsOrColumns == 0
                                           ? 0
                                           : (int) Math.Ceiling(Count / (float) RowsOrColumns);

        public bool HasItems => Count > 0;

        public int? LastIndex => HasItems
                                     ? Count - 1
                                     : (int?) null;

        public int? FirstIndex => HasItems
                                      ? 0
                                      : (int?) null;

        #endregion

        #region Protected/private fields/properties

        private readonly ChoiceHelper _choiceHelper;
        protected readonly List<IView> _views = new List<IView>();

        private readonly ReactiveProperty<ReadOnlyCollection<IView>> _reactiveViews =
            new ReactiveProperty<ReadOnlyCollection<IView>>(new ReadOnlyCollection<IView>(Array.Empty<IView>()));

        private VariantSet _variantSet;

        private readonly ReactiveProperty<List<object>>
            _models = new ReactiveProperty<List<object>>(new List<object>());

        private int? _startIndex;

        protected VariantSet VariantSet =>
            _variantSet ?? (_variantSet = VariantProvider.GetVariantsNamed(Variants));

        #endregion

        #region Constructor

        public ListControl()
        {
            _choiceHelper = new ChoiceHelper(this);
            Views = _reactiveViews.ToReadOnlyReactiveProperty();
            Models = _models.Select(x => new ReadOnlyCollection<object>(x))
                            .ToReadOnlyReactiveProperty();
        }

        protected virtual void Start()
        {
            _choiceHelper.Start();
        }

        #endregion

        #region IPresenter members

        [Pure]
        protected override IObservable<IView> PresentView(object input, IOptions options = null)
        {
            MutableState.Value = PresenterState.Loading;

            // If input is observable, resolve it first
            if (input is IObservable<object> observable)
                return observable.ContinueWith(x => PresentView(x, options));

            options = options.With(VariantProvider.GetVariantsNamed(Variants));

            return Observable.Defer(
                () => PresentInternal(input, options)
                   .DoOnCompleted(
                        () =>
                        {
                            if (AutoChooseFirst)
                                _choiceHelper.ChooseFirst();
                        }));
        }

        #endregion

        #region Selection

        public void OnMove(AxisEventData eventData)
        {
            _choiceHelper.OnMove(eventData);
        }

        public bool Handle(IRequest request) => _choiceHelper.Handle(request);

        public override GameObject SelectableContent => _choiceHelper.ChosenView.Value?.GameObject;

        public IReadOnlyReactiveProperty<IView> ChosenView => _choiceHelper.ChosenView;

        public ReactiveProperty<int?> ChosenIndex => _choiceHelper.ChosenIndex;

        public IReactiveProperty<object> ChosenModel => _choiceHelper.ChosenModel;

        public void ChooseView(IView view)
        {
            _choiceHelper.ChooseView(view);
        }

        public void ChooseView<TView>(Func<TView, bool> predicate) where TView : IView
        {
            _choiceHelper.ChooseView(predicate);
        }

        public void ChooseViewModel<TViewModel>(TViewModel viewModel) where TViewModel : IViewModel
        {
            _choiceHelper.ChooseViewModel(viewModel);
        }

        public void ChooseViewModel<TViewModel>(Func<TViewModel, bool> predicate) where TViewModel : IViewModel
        {
            _choiceHelper.ChooseViewModel(predicate);
        }

        public void ChooseModel<TModel>(TModel model)
        {
            _choiceHelper.ChooseModel(model);
        }

        public void ChooseModel<TModel>(Func<TModel, bool> predicate)
        {
            _choiceHelper.ChooseModel(predicate);
        }

        public bool ChooseIndex(int index)
        {
            return _choiceHelper.ChooseIndex(index);
        }

        public bool ChooseFirst()
        {
            return _choiceHelper.ChooseFirst();
        }

        public bool ChooseLast()
        {
            return _choiceHelper.ChooseLast();
        }

        public void ChooseNone()
        {
            _choiceHelper.ChooseNone();
        }

        public bool ChoosePrevious()
        {
            return _choiceHelper.ChoosePrevious();
        }

        public bool ChooseNext()
        {
            return _choiceHelper.ChooseNext();
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
            index.HasValue
                ? _views.Where(view => !ExcludeInactiveGameObjects || view.GameObject.activeInHierarchy)
                        .GetAtOrDefault(index.Value)
                : null;

        [Pure]
        public IObservable<IView> Add(object input, IOptions options = null) =>
            RecipeProvider.GetRecipe(input, options)
                          .ContinueWith(
                               x => LoadView(x)
                                  .Do(
                                       view =>
                                       {
                                           AddView(_views.Count, view);
                                           UpdateReactiveViews();
                                       }));

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

            _choiceHelper.ChooseNone();
        }

        #endregion

        #region Private methods

        protected virtual IObservable<IView> PresentInternal(object input, IOptions options)
        {
            if (!AddBeforeExistingObject && _startIndex == null)
                _startIndex = Content.transform.childCount;

            var models = (input as List<object>)?.ToList() ?? (input as IEnumerable)?.Cast<object>()
                                                                                     .ToList() ??
                         input?.ToSingleItemList() ?? new List<object>();

            _models.Value = models;
            RemoveViews(Content, _views);
            _views.Clear();
            _reactiveViews.Value = new ReadOnlyCollection<IView>(Array.Empty<IView>());

            var entries = models.Select((x, i) => new Entry(i, x))
                                .ToList();

            return LoadViews(entries, options);
        }

        protected virtual IObservable<IView> LoadViews(List<Entry> entries, IOptions options) =>
            LoadAllViews(entries, options)
               .Do(
                    x =>
                    {
                        MutableState.Value = PresenterState.Presenting;
                        AddView(x.Index, x.View);
                    })
               .Select(x => x.View);

        private IObservable<Entry> LoadAllViews(List<Entry> entries, IOptions options)
        {
            return entries.ToObservable()
                          .SelectMany(
                               entry => RecipeProvider.GetRecipe(entry.Model, options)
                                                      .Do(recipe => entry.Recipe = recipe)
                                                      .Select(x => entry))
                          .SelectMany(
                               entry => LoadView(entry.Recipe.Value)
                                       .Do(view => entry.View = view)
                                       .Select(_ => entry))
                          .DoOnCompleted(
                               () =>
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
                    if (ViewModelComparer.Compare(
                            view.ViewModel,
                            _views[i]
                               .ViewModel) < 0)
                        return i;

                return null;
            }

            if (ModelComparer != null)
            {
                for (var i = 0; i < _views.Count; i++)
                    if (ModelComparer.Compare(
                            view.ViewModel?.Model,
                            _views[i]
                               .ViewModel?.Model) < 0)
                        return i;
            }

            return null;
        }

        #endregion

        #region Protected/virtual methods

        protected virtual void AddView(int index, IView view)
        {
            var sortedIndex = GetSortedIndex(view);
            if (sortedIndex != null)
            {
                InsertView(Content, sortedIndex.Value + (_startIndex ?? 0), view);
                _views.Insert(sortedIndex.Value, view);
            }
            else
            {
                if (_views.Count < index + 1)
                    _views.AddRange(Enumerable.Repeat<IView>(null, index + 1 - _views.Count));

                InsertView(Content, index + (_startIndex ?? 0), view);
                _views[index] = view;
            }
        }

        protected virtual IObservable<IView> LoadView(Recipe recipe) =>
            ViewLoader.Load(GetInstantiationContainer(), recipe, Container, CancellationToken.None);

        #endregion
    }
}