using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Silphid.Sequencit;
using Silphid.Extensions;
using Silphid.Injexit;
using Silphid.Requests;
using Silphid.Showzup.Requests;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Silphid.Showzup
{
    public class NavigationControl : TransitionControl, INavigationPresenter, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NavigationControl));

        #region Fields

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly Subject<NavPresentation> _navigating = new Subject<NavPresentation>();
        private readonly Subject<NavPresentation> _navigated = new Subject<NavPresentation>();
        private ReadOnlyReactiveProperty<bool> _canPush;
        private ReadOnlyReactiveProperty<bool> _canPop;

        #endregion

        public GameObject HistoryContainer;
        public bool CanPopTopLevelView;

        [FormerlySerializedAs("ShouldHandleBackRequests")] [FormerlySerializedAs("HandlesBackRequest")]
        public bool HandleBackRequest;

        #region Life-time

        [Inject]
        public void Inject()
        {
            History.PairWithPrevious()
                .Skip(1)
                .Subscribe(
                    x =>
                    {
                        UpdateHistoryChanged(x);
                        DestroyInvalidatedViews(x.Item2);
                    })
                .AddTo(_disposables);
        }

        private void Awake()
        {
            HistoryContainer.SetActive(false);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        #endregion

        #region INavigationPresenter members

        public ReadOnlyReactiveProperty<bool> CanPresent =>
            _canPush ??
            (_canPush = this.Ready()
                .ToReadOnlyReactiveProperty());

        public ReadOnlyReactiveProperty<bool> CanPop =>
            _canPop ??
            (_canPop = History.Select(
                    x => x.Count >
                         (CanPopTopLevelView
                             ? 0
                             : 1))
                .DistinctUntilChanged()
                .CombineLatest(this.Ready(), (x, y) => x && y)
                .ToReadOnlyReactiveProperty());

        public IReadOnlyReactiveProperty<Nav> RootHistory => History.Select(x => x.FirstOrDefault())
            .ToReadOnlyReactiveProperty();

        public ReactiveProperty<List<Nav>> History { get; } = new ReactiveProperty<List<Nav>>(new List<Nav>());

        public IObservable<NavPresentation> Navigating => _navigating;
        public IObservable<NavPresentation> Navigated => _navigated;

        protected override void RemoveView(GameObject viewObject)
        {
            if (HistoryContainer == null)
            {
                base.RemoveView(viewObject);
                return;
            }

            viewObject.transform.SetParent(HistoryContainer.transform, false);
        }

        #region Navigation

        private IObservable<Presentation> CreatePresentationFromInput(object input, IOptions options)
        {
            if (input is IObservable<object> observable)
                return observable.ContinueWith(x => CreatePresentationFromInput(x, options));

            return RecipeProvider.GetRecipe(input, options)
                .ContinueWith(recipe => LoadView(recipe)
                    .Select(view =>
                    {
                        var presentation = CreatePresentation(recipe.ViewModel, _view.Value, recipe.ViewType, options);
                        presentation.TargetView = view;
                        return presentation;
                    }))
                .DoOnCompleted(() => MutableState.Value = PresenterState.Presenting);
        }

        private Presentation CreatePresentationFromView(IView view, IOptions options)
        {
            var presentation = CreatePresentation(null, _view.Value, view?.GetType(), options);
            presentation.TargetView = view;
            return presentation;
        }

        private IObservable<IView> Navigate(Presentation presentation)
        {
            var nav = StartNavigation(presentation);

            return Completable.WhenAll(Present(presentation), nav.Parallel)
                .DoOnCompleted(
                    () =>
                    {
                        CompleteNavigation(nav);
                        CompleteChange(nav);
                    })
                .ThenReturn(presentation.TargetView);
        }

        private List<Nav> GetNewHistory(Nav nav, PushMode pushMode) =>
            pushMode == PushMode.Child
                ? History.Value.Append(nav)
                    .ToList()
                : pushMode == PushMode.Sibling
                    ? History.Value.Take(History.Value.Count - 1)
                        .Append(nav)
                        .ToList()
                    : new List<Nav> {nav};

        #endregion

        #region Push

        protected override IObservable<IView> PresentView(object input, IOptions options = null) =>
            Observable.Defer(() => PresentInternal(input, options.With(VariantProvider.GetVariantsNamed(Variants))));

        private IObservable<IView> PresentInternal(object input, IOptions options)
        {
            AssertCanPresent(input, options);
            StartChange();

            var originalInput = options.GetHistoryInput() ?? input;

            return CreatePresentationFromInput(input, options)
                .ContinueWith(Navigate)
                .Do(
                    x => History.Value = GetNewHistory(
                        new Nav(originalInput, x, options),
                        options.GetPushMode() ?? PushMode.Default));
        }

        #endregion

        #region Pop

        public IObservable<IView> Pop()
        {
            return Observable.Defer(
                () =>
                {
                    AssertCanPop();

                    var nav = History.Value.Count >= 2
                        ? History.Value[History.Value.Count - 2]
                        : null;

                    var newHistory = History.Value.Take(History.Value.Count - 1)
                        .ToList();

                    Log.Debug($"Pop({(object) nav?.View})");

                    return PopInternal(nav, newHistory);
                });
        }

        public IObservable<IView> PopToRoot()
        {
            return Observable.Defer(
                () =>
                {
                    AssertCanPop();

                    var nav = CanPopTopLevelView
                        ? null
                        : History.Value.First();

                    var newHistory = CanPopTopLevelView
                        ? new List<Nav>()
                        : History.Value.Take(1)
                            .ToList();

                    Log.Debug($"PopToRoot({(object) nav?.View})");

                    return PopInternal(nav, newHistory);
                });
        }

        public IObservable<IView> PopTo(Nav nav)
        {
            return Observable.Defer(
                () =>
                {
                    var historyIndex = History.Value.IndexOf(nav);

                    AssertCanPopTo(nav, historyIndex);

                    var newHistory = History.Value.Take(historyIndex + 1)
                        .ToList();

                    return PopInternal(nav, newHistory);
                });
        }

        private IObservable<IView> PopInternal(Nav nav, List<Nav> history)
        {
            if (nav == null)
                nav = Nav.Empty;

            AssertCanPresent(nav, nav.Options);
            StartChange();

            var options = nav.Options.With(Direction.Backward);

            IObservable<Presentation> presentationObservable;
            lock (this)
            {
                presentationObservable = nav.IsInvalid
                    ? CreatePresentationFromInput(nav.Input, options)
                    : Observable.Return(CreatePresentationFromView(nav.View, options));
                nav.Validate();
            }

            return presentationObservable.ContinueWith(Navigate)
                .Do(
                    x =>
                    {
                        nav.View = x;
                        nav.ViewType = x.GetType();
                        History.Value = history;
                    });
        }

        #endregion

        #endregion

        #region Implementation

        private void StartChange()
        {
            MutableState.Value = PresenterState.Loading;
        }

        private NavPresentation StartNavigation(Presentation presentation)
        {
            var navPresentation = new NavPresentation(
                presentation.SourceView,
                presentation.TargetView,
                presentation.Options,
                presentation.Transition,
                presentation.Duration,
                Parallel.Create());

            _navigating.OnNext(navPresentation);
            _view.Value = null;
            return navPresentation;
        }

        private void CompleteNavigation(NavPresentation navPresentation)
        {
            _view.Value = navPresentation.Target;
            _navigated.OnNext(navPresentation);
        }

        private void CompleteChange(NavPresentation navPresentation)
        {
            MutableState.Value = PresenterState.Ready;
            (navPresentation.Target as IPostNavigate)?.OnPostNavigate();
        }

        private void AssertCanPresent(object input, IOptions options)
        {
            if (!CanPresent.Value)
                throw new PresentException(gameObject, input, options, "Cannot present at this moment");
        }

        private void AssertCanPop()
        {
            if (!CanPop.Value)
                throw new PopException(gameObject, "Cannot pop at this moment");
        }

        private void AssertCanPopTo(Nav nav, int historyIndex)
        {
            AssertCanPop();

            if (historyIndex < 0)
            {
                var navStr = nav?.ToString() ?? "null";
                throw new PopException(gameObject, $"History does not contain {navStr}");
            }

            if (historyIndex == History.Value.Count - 1)
                throw new PopException(gameObject, $"Cannot pop to {nav} because it is already current view");
        }

        private void UpdateHistoryChanged(Tuple<List<Nav>, List<Nav>> tuple)
        {
            tuple.Item1.Where(x => !tuple.Item2.Contains(x))
                .ForEach(x => x.DestroyView());
        }

        private void DestroyInvalidatedViews(IEnumerable<Nav> history)
        {
            history.Where(x => x.IsInvalid)
                .ForEach(x => x.DestroyView());
        }

        #endregion

        #region IRequestHandler members

        public override bool Handle(IRequest request)
        {
            if (base.Handle(request))
                return true;

            var backRequest = request as BackRequest;
            if (backRequest == null || !HandleBackRequest || !CanPop.Value)
                return false;

            Pop()
                .SubscribeAndForget();
            return true;
        }

        #endregion
    }
}