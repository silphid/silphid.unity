using System;
using System.Threading;
using JetBrains.Annotations;
using log4net;
using Silphid.Extensions;
using Silphid.Injexit;
using Silphid.Requests;
using Silphid.Showzup.Navigation;
using Silphid.Showzup.Recipes;
using Silphid.Showzup.Requests;
using UniRx;
using UnityEngine;
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
            public readonly IOptions Options;
            public readonly Subject<IView> Subject = new Subject<IView>();

            public PendingRequest(object input, IOptions options)
            {
                Input = input;
                Options = options;
            }
        }

        #endregion

        #region Injected properties

        [Inject, UsedImplicitly] internal IViewLoader ViewLoader { get; set; }

        #endregion

        #region Properties

        public string[] Variants;
        public ReadOnlyReactiveProperty<IView> View { get; }

        [FormerlySerializedAs("ShouldHandlePresentRequests")] [FormerlySerializedAs("HandlesPresentRequest")]
        public bool HandlePresentRequest;

        protected VariantSet VariantSet =>
            _variantSet ?? (_variantSet = VariantProvider.GetVariantsNamed(Variants));

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

        public override GameObject SelectableContent
        {
            get { return View.Value?.GameObject; }
        }

        protected virtual void Start()
        {
            View.WhereNotNull()
                .Subscribe(
                     x =>
                     {
                         if (IsSelfOrDescendantSelected.Value)
                             x.Select();
                     })
                .AddTo(this);
        }

        #region IPresenter members

        protected override IObservable<IView> PresentView(object input, IOptions options = null)
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

        private IObservable<IView> PresentNow(object input, IOptions options)
        {
            MutableState.Value = PresenterState.Loading;

            // If input is observable, resolve it first
            if (input is IObservable<object> observable)
                return observable.SelectMany(x => PresentNow(x, options));

            return RecipeProvider.GetRecipe(input, options)
                .ContinueWith(recipe => LoadView(recipe)
                    .Select(view =>
                    {
                        var presentation = CreatePresentation(recipe.ViewModel, _view.Value, recipe.ViewType, options);
                        presentation.TargetView = view;
                        return presentation;
                    }))
                .ContinueWith(presentation =>
                {
                    MutableState.Value = PresenterState.Presenting;
                    return Present(presentation)
                        .ThenReturn(presentation.TargetView);
                })
                .DoOnCompleted(CompleteRequest);
        }

        private IObservable<IView> PresentLater(object input, IOptions options)
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

        protected IObservable<IView> LoadView(Recipe recipe)
        {
            Log.Debug($"Loading: {recipe}");

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellations = _loadCancellations.Do(_ => cancellationTokenSource.Cancel());
            return ViewLoader.Load(GetInstantiationContainer(), recipe, Container, cancellationTokenSource.Token)
                             .TakeUntil(cancellations);
        }

        protected virtual Presentation CreatePresentation(object viewModel,
                                                          IView sourceView,
                                                          Type targetViewType,
                                                          IOptions options) =>
            new Presentation(viewModel, sourceView, targetViewType, options);

        #endregion

        #region Virtual and abstract members

        protected abstract ICompletable Present(Presentation presentation);

        #endregion

        #region IRequestHandler members

        public virtual bool Handle(IRequest request)
        {
            var presentRequest = request as PresentRequest;
            if (presentRequest != null && HandlePresentRequest)
            {
                Present(presentRequest.Input, presentRequest.Options)
                   .SubscribeAndForget();
                return true;
            }

            return false;
        }

        #endregion
    }
}