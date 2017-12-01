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
        private readonly Subject<Nav> _navigating = new Subject<Nav>();
        private readonly Subject<Nav> _navigated = new Subject<Nav>();
        private ReadOnlyReactiveProperty<bool> _canPush;
        private ReadOnlyReactiveProperty<bool> _canPop;

        #endregion

        public GameObject HistoryContainer;
        public bool CanPopTopLevelView;
        
        [FormerlySerializedAs("HandlesBackRequest")]
        public bool HandleBackRequest;

        #region Life-time

        [Inject]
        public void Inject()
        {
            History.PairWithPrevious().Subscribe(DisposeDroppedViews).AddTo(_disposables);
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
            _canPush ?? (_canPush = this.Ready().ToReadOnlyReactiveProperty());

        public ReadOnlyReactiveProperty<bool> CanPop =>
            _canPop ?? (_canPop = History
                .Select(x => x.Count > (CanPopTopLevelView ? 0 : 1))
                .DistinctUntilChanged()
                .CombineLatest(this.Ready(), (x, y) => x && y)
                .ToReadOnlyReactiveProperty());

        public IReadOnlyReactiveProperty<IView> RootView => History
                .Select(x => x.FirstOrDefault())
                .ToReadOnlyReactiveProperty();

        public ReactiveProperty<List<IView>> History { get; } =
            new ReactiveProperty<List<IView>>(new List<IView>());

        public IObservable<Nav> Navigating => _navigating;
        public IObservable<Nav> Navigated => _navigated;

        protected override void RemoveView(GameObject viewObject)
        {
            if (HistoryContainer == null)
            {
                base.RemoveView(viewObject);
                return;
            }

            viewObject.transform.SetParent(HistoryContainer.transform, false);
        }

        protected override IObservable<IView> PresentView(object input, Options options = null)
        {
            options = options.With(VariantProvider.GetVariantsNamed(Variants));

            return Observable
                .Defer(() => StartPushAndLoadView(input, options))
                .ContinueWith(NavigateAndCompletePush)
                .DoOnTerminate(CompleteChange);
        }

        private IObservable<Presentation> StartPushAndLoadView(object input, Options options)
        {
            var observable = input as IObservable<object>;
            if (observable != null)
                return observable.SelectMany(x => StartPushAndLoadView(x, options));
            AssertCanPresent(input, options);

            var viewInfo = ResolveView(input, options);
            var presentation = CreatePresentation(viewInfo.ViewModel, _view.Value, viewInfo.ViewType, options);

            StartChange();

            return LoadView(viewInfo)
                .DoOnCompleted(() => MutableState.Value = PresenterState.Presenting)
                .Do(view => presentation.TargetView = view)
                .ThenReturn(presentation);
        }

        private IObservable<IView> NavigateAndCompletePush(Presentation presentation)
        {
            var nav = StartNavigation(presentation);

            return Observable
                .WhenAll(
                    Present(presentation),
                    nav.Parallel)
                .DoOnCompleted(() =>
                {
                    History.Value = GetNewHistory(presentation.TargetView, presentation.Options.GetPushModeOrDefault());
                    CompleteNavigation(nav);
                })
                .ThenReturn(presentation.TargetView);
        }

        private List<IView> GetNewHistory(IView view, PushMode pushMode) =>
            pushMode == PushMode.Child
                ? History.Value.Append(view).ToList()
                : pushMode == PushMode.Sibling
                    ? History.Value.Take(History.Value.Count - 1).Append(view).ToList()
                    : new List<IView> {view};

        public IObservable<IView> Pop()
        {
            AssertCanPop();

            var view = History.Value.Count >= 2
                ? History.Value[History.Value.Count - 2]
                : null;

            Log.Debug($"Pop({view})");
            var history = History.Value.Take(History.Value.Count - 1).ToList();

            return PopInternal(view, history);
        }

        public IObservable<IView> PopToRoot()
        {
            AssertCanPop();

            var view = History.Value.First();
            Log.Debug($"PopToRoot({view})");
            var history = History.Value.Take(1).ToList();

            return PopInternal(view, history);
        }

        public IObservable<IView> PopTo(IView view)
        {
            Log.Debug($"PopTo({view})");
            var viewIndex = History.Value.IndexOf(view);
            AssertCanPopTo(view, viewIndex);
            var history = History.Value.Take(viewIndex + 1).ToList();

            return PopInternal(view, history);
        }

        private IObservable<IView> PopInternal(IView view, List<IView> history)
        {
            AssertCanPresent(null, null);

            StartChange();

            var options = new Options {Direction = Direction.Backward};
            var presentation = CreatePresentation(null, _view.Value, view?.GetType(), options);
            presentation.TargetView = view;
            var nav = StartNavigation(presentation);

            return Observable
                .WhenAll(
                    PerformTransition(presentation),
                    nav.Parallel)
                .DoOnCompleted(() =>
                {
                    History.Value = history;
                    CompleteNavigation(nav);
                    CompleteChange();
                })
                .ThenReturn(view);
        }

        #endregion

        #region Implementation

        private void StartChange()
        {
            MutableState.Value = PresenterState.Loading;
        }

        private Nav StartNavigation(Presentation presentation)
        {
            var nav = new Nav(
                presentation.SourceView,
                presentation.TargetView,
                new Parallel(),
                presentation.Transition,
                presentation.Duration);

            _navigating.OnNext(nav);
            _view.Value = null;
            return nav;
        }

        private void CompleteNavigation(Nav nav)
        {
            _view.Value = nav.Target;
            _navigated.OnNext(nav);
        }

        private void CompleteChange()
        {
            MutableState.Value = PresenterState.Ready;
        }

        private void AssertCanPresent(object input, Options options)
        {
            if (!CanPresent.Value)
                throw new PresentException(gameObject, input, options, "Cannot present at this moment");
        }

        private void AssertCanPop()
        {
            if (!CanPop.Value)
                throw new PopException(gameObject, "Cannot pop at this moment");
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void AssertCanPopTo(IView view, int viewIndex)
        {
            AssertCanPop();

            if (viewIndex == -1)
                throw new PopException(gameObject, $"History does not contain view {view}");
            if (viewIndex == History.Value.Count - 1)
                throw new PopException(gameObject, $"Cannot pop to view {view} because it is already current view");
        }

        private void DisposeDroppedViews(Tuple<List<IView>, List<IView>> tuple)
        {
            tuple.Item1
                .Where(x => !tuple.Item2.Contains(x))
                .ForEach(DisposeView);
        }

        private void DisposeView(IView view)
        {
            if (view == null)
                return;

            Destroy(view.GameObject);
            (view as IDisposable)?.Dispose();
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

            Pop().SubscribeAndForget();
            return true;
        }

        #endregion
    }
}