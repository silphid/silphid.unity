using System;
using System.Threading;
using JetBrains.Annotations;
using log4net;
using Silphid.Extensions;
using Silphid.Injexit;
using Silphid.Requests;
using Silphid.Showzup.Requests;
using UniRx;
using UnityEngine.Serialization;

namespace Silphid.Showzup
{
    public abstract class SinglePresenterControl : PresenterControl, IRequestHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SinglePresenterControl));
        
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

        [Inject, UsedImplicitly] internal IViewResolver ViewResolver { get; set; }
        [Inject, UsedImplicitly] internal IViewLoader ViewLoader { get; set; }
        [Inject, UsedImplicitly] internal IVariantProvider VariantProvider { get; set; }

        #endregion

        #region Properties

        public string[] Variants;
        public ReadOnlyReactiveProperty<IView> View { get; }
        
        [FormerlySerializedAs("ShouldHandlePresentRequests")]
        [FormerlySerializedAs("HandlesPresentRequest")]
        public bool HandlePresentRequest;

        protected VariantSet VariantSet =>
            _variantSet ??
            (_variantSet = VariantProvider.GetVariantsNamed(Variants));

        #endregion

        #region Private fields

        private readonly Subject<Unit> _loadCancellations = new Subject<Unit>();
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

        protected override IObservable<IView> PresentView(object input, Options options = null)
        {
            options = options.With(VariantProvider.GetVariantsNamed(Variants));

            if (MutableState.Value == PresenterState.Ready)
                return PresentNow(input, options);

            if (MutableState.Value == PresenterState.Loading)
            {
                CancelLoading();
                return PresentNow(input, options);
            }

            // PresenterState.Presenting
            return PresentLater(input, options);
        }

        #endregion

        #region Implementation

        private IObservable<IView> PresentNow(object input, Options options)
        {
            MutableState.Value = PresenterState.Loading;
            
            // If input is observable, resolve it first
            var observable = input as IObservable<object>;
            if (observable != null)
                return observable.SelectMany(x => PresentNow(x, options));

            var viewInfo = ResolveView(input, options);
            var presentation = CreatePresentation(viewInfo.ViewModel, _view.Value, viewInfo.ViewType, options);

            return Observable
                .Defer(() => LoadView(viewInfo))
                .ContinueWith(view =>
                {
                    MutableState.Value = PresenterState.Presenting;
                    Log.Debug($"Presenting: {viewInfo}");
                    presentation.TargetView = view;
                    return Present(presentation).ThenReturn(view);
                })
                .DoOnError(_ => MutableState.Value = PresenterState.Ready)
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
            MutableState.Value = PresenterState.Ready;
            _loadCancellations.OnNext(Unit.Default);
        }

        private void CompleteRequest()
        {
            if (_pendingRequest != null)
            {
                PresentNow(_pendingRequest.Input, _pendingRequest.Options)
                    .SubscribeAndForget(_pendingRequest.Subject);

                _pendingRequest = null;
            }
            else
            {
                MutableState.Value = PresenterState.Ready;
            }
        }

        protected IObservable<IView> LoadView(ViewInfo viewInfo)
        {
            Log.Debug($"Loading: {viewInfo}");

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellations = _loadCancellations.Do(_ => cancellationTokenSource.Cancel());
            return ViewLoader
                .Load(GetInstantiationContainer(), viewInfo, cancellationTokenSource.Token)
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

        public virtual bool Handle(IRequest request)
        {
            var presentRequest = request as PresentRequest;
            if (presentRequest != null && HandlePresentRequest)
            {
                Present(presentRequest.Input, presentRequest.Options).SubscribeAndForget();
                return true;
            }

            return false;
        }

        #endregion
    }
}