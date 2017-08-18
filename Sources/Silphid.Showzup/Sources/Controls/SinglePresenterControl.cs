using System;
using System.Linq;
using Silphid.Extensions;
using Silphid.Injexit;
using Silphid.Showzup.Requests;
using UniRx;

namespace Silphid.Showzup
{
    //TODO to rename. 
    public abstract class SinglePresenterControl : PresenterControl, IRequestHandler
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
        [Inject] internal IVariantProvider VariantProvider { get; set; }

        #endregion

        #region Properties

        public string[] Variants;
        public ReadOnlyReactiveProperty<IView> View { get; }
        public bool ShouldHandlePresentRequests;

        protected VariantSet VariantSet =>
            _variantSet ??
            (_variantSet = VariantProvider.GetVariantsNamed(Variants));

        #endregion

        #region Private fields

        private readonly Subject<Unit> _loadCancellations = new Subject<Unit>();
        private State _state;
        private PendingRequest _pendingRequest;
        protected readonly ReactiveProperty<IView> _view = new ReactiveProperty<IView>();
        private VariantSet _variantSet;

        #endregion

        protected SinglePresenterControl()
        {
            View = _view.ToReadOnlyReactiveProperty();
            View.Subscribe(x => MutableFirstView.Value = x);
        }

        #region IPresenter members

        public override IObservable<IView> Present(object input, Options options = null)
        {
            var observable = input as IObservable<object>;
            if (observable != null)
                return observable.SelectMany(x => Present(x, options));
            
            options = Options.CloneWithExtraVariants(options, VariantProvider.GetVariantsNamed(Variants));
            
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

        private IObservable<IView> PresentNow(object input, Options options)
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

        private IObservable<IView> PresentLater(object input, Options options)
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

        protected IObservable<IView> LoadView(ViewInfo viewInfo, Options options)
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

        protected abstract IObservable<Unit> Present(Presentation presentation);

        #endregion

        #region IRequestHandler members

        public bool Handle(IRequest request)
        {
            var presentRequest = request as PresentRequest;
            if (presentRequest != null && ShouldHandlePresentRequests)
            {
                Present(presentRequest.Input, presentRequest.Options).SubscribeAndForget();
                return true;
            }
            
            return false;
        }

        #endregion
    }
}