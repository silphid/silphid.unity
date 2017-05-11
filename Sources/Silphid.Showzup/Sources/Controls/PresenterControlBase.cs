using System;
using Silphid.Extensions;
using Silphid.Sequencit;
using UniRx;
using Rx = UniRx;
using Zenject;

namespace Silphid.Showzup
{
    public abstract class PresenterControlBase : Control, IPresenter
    {
        #region State enum

        protected enum State
        {
            Ready,
            Loading,
            Presenting
        }

        #endregion

        #region PendingRequest inner-class

        private class PendingRequest
        {
            public readonly object Input;
            public readonly Options Options;
            public readonly Subject<IView> Subject = new Subject<IView>();

            public PendingRequest(object input, Options options)
            {
                Input = input;
                Options = options;
            }
        }

        #endregion

        #region Injected properties

        [Inject] internal IViewResolver ViewResolver { get; set; }
        [Inject] internal IViewLoader ViewLoader { get; set; }

        #endregion

        #region Config properties

        public string[] Variants;

        #endregion

        #region Private fields

        private readonly Subject<Unit> _loadCancellations = new Subject<Unit>();
        private State _state;
        private PendingRequest _pendingRequest;

        protected readonly ReactiveProperty<IView> _view = new ReactiveProperty<IView>();
        public ReadOnlyReactiveProperty<IView> View { get; }

        #endregion

        protected PresenterControlBase()
        {
            View = _view.ToReadOnlyReactiveProperty();
        }

        #region IPresenter members

        public virtual Rx.IObservable<IView> Present(object input, Options options = null)
        {
            options = Options.CloneWithExtraVariants(options, Variants);

            if (_state == State.Ready)
                return PresentNow(input, options);

            if (_state == State.Loading)
            {
                CancelLoading();
                return PresentNow(input, options);
            }

            // State.Presenting
            return PresentLater(input, options);
        }

        #endregion

        #region Implementation

        private Rx.IObservable<IView> PresentNow(object input, Options options)
        {
            var viewInfo = ResolveView(input, options);
            var presentation = CreatePresentation(viewInfo.ViewModel, _view.Value, viewInfo.ViewType, options);

            _state = State.Loading;
            return Observable
                .Defer(() => LoadView(viewInfo, options))
                .DoOnError(_ => _state = State.Ready)
                .ContinueWith(view =>
                {
                    _state = State.Presenting;
                    presentation.TargetView = view;
                    return Present(presentation)
                        .ThenReturn(view);
                })
                .DoOnCompleted(CompleteRequest);
        }

        private Rx.IObservable<IView> PresentLater(object input, Options options)
        {
            // Complete any pending request without fulling it (we only allow a single pending request)
            _pendingRequest?.Subject.OnCompleted();

            // Prepare new pending request
            _pendingRequest = new PendingRequest(input, options);
            return _pendingRequest.Subject;
        }

        private void CancelLoading()
        {
            _state = State.Ready;
            _loadCancellations.OnNext(Unit.Default);
        }

        private void CompleteRequest()
        {
            _state = State.Ready;

            if (_pendingRequest != null)
            {
                PresentNow(_pendingRequest.Input, _pendingRequest.Options)
                    .SubscribeAndForget(_pendingRequest.Subject);

                _pendingRequest = null;
            }
        }

        protected Rx.IObservable<IView> LoadView(ViewInfo viewInfo, Options options)
        {
            var cancellationDisposable = new BooleanDisposable();
            var cancellationToken = new CancellationToken(cancellationDisposable);
            var cancellations = _loadCancellations.Do(_ => cancellationDisposable.Dispose());

            return ViewLoader
                .Load(viewInfo, cancellationToken)
                .TakeUntil(cancellations);
        }

        protected virtual Presentation CreatePresentation(object viewModel, IView sourceView, Type targetViewType, Options options) =>
            new Presentation(viewModel, sourceView, targetViewType, options);

        protected ViewInfo ResolveView(object input, Options options) =>
            ViewResolver.Resolve(input, options);

        #endregion

        #region Virtual and abstract members

        protected abstract Rx.IObservable<Unit> Present(Presentation presentation);

        #endregion
    }
}