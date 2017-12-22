using System;
using JetBrains.Annotations;
using log4net;
using Silphid.Sequencit;
using Silphid.Extensions;
using Silphid.Injexit;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public class TransitionControl : SinglePresenterControl
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TransitionControl));

        #region Fields

        private GameObject _sourceContainer;
        private GameObject _targetContainer;

        #endregion

        #region Properties

        public GameObject Container1;
        public GameObject Container2;
        public Components.Transition DefaultTransition;
        public bool TransitionInstantlyFromNull;
        public bool TransitionInstantlyToNull;

        #endregion

        #region Injected properties

        [Inject, Optional, UsedImplicitly]
        private ITransitionResolver TransitionResolver { get; set; }

        #endregion

        #region Virtual and abstract overrides

        protected override ICompletable Present(Presentation presentation)
        {
            return PerformTransition(presentation);
        }

        #endregion

        #region Implementation

        protected override Presentation CreatePresentation(object viewModel, IView sourceView, Type targetViewType,
            Options options)
        {
            var presentation = base.CreatePresentation(viewModel, sourceView, targetViewType, options);
            presentation.Transition = ResolveTransition(presentation, options);
            presentation.Duration = ResolveDuration(presentation.Transition, options);
            return presentation;
        }

        protected ITransition ResolveTransition(Presentation presentation, Options options) =>
            options?.Transition ??
            TransitionResolver?.Resolve(presentation) ??
            DefaultTransition ??
            InstantTransition.Instance;

        protected float ResolveDuration(ITransition transition, Options options) =>
            options?.TransitionDuration ??
            transition.Duration;

        protected ICompletable PerformTransition(Presentation presentation)
        {
            var sourceView = presentation.SourceView;
            var targetView = presentation.TargetView;
            var options = presentation.Options;
            var transition = presentation.Transition;
            var duration = presentation.Duration;

            if (!gameObject.activeInHierarchy ||
                sourceView == null && TransitionInstantlyFromNull ||
                targetView == null && TransitionInstantlyToNull)
            {
                transition = InstantTransition.Instance;
                duration = 0f;
            }

            return Sequence
                .Create(seq =>
                {
                    seq.AddAction(() => PrepareContainers(presentation));

                    PreHide(sourceView, options, seq);
                    Deconstruct(sourceView, options, seq);
                    PreShow(targetView, options, seq);

                    seq.Add(() => Observable.NextFrame());
                    seq.Add(() =>
                        transition.Perform(_sourceContainer, _targetContainer, options.GetDirectionOrDefault(),
                            duration));

                    PostHide(sourceView, options, seq);
                    Construct(targetView, options, seq);
                    PostShow(targetView, options, seq);

                    seq.AddAction(() => CompleteTransition(presentation));
                })
                .DoOnError(ex =>
                {
                    if (ex is TransitionParentDestroyedException)
                        Log.Error("TransitionControl has been destroyed");
                    else if (targetView != null && targetView.GameObject != null)
                        targetView.GameObject.Destroy();

                    Log.Error(
                        new Exception($"Failed to transition from {sourceView} to {targetView}.", ex));
                });
        }

        private void PreHide(IView view, Options options, ISequencer seq)
        {
            var preHide = view as IPreHide;
            if (preHide != null)
                seq.AddAction(() => preHide.OnPreHide(options));
        }

        private void PreShow(IView view, Options options, ISequencer seq)
        {
            var preShow = view as IPreShow;
            if (preShow != null)
                seq.AddAction(() => preShow.OnPreShow(options));
        }

        private void Deconstruct(IView view, Options options, ISequencer seq)
        {
            var deconstructable = view as IDeconstructable;
            if (deconstructable != null)
                seq.Add(() => deconstructable.Deconstruct(options));
        }

        private void Construct(IView view, Options options, ISequencer seq)
        {
            var constructable = view as IConstructable;
            if (constructable != null)
                seq.Add(() => constructable.Construct(options));
        }

        private void PostShow(IView view, Options options, ISequencer seq)
        {
            var postShow = view as IPostShow;
            if (postShow != null)
                seq.AddAction(() => postShow.OnPostShow(options));
        }

        private void PostHide(IView view, Options options, ISequencer seq)
        {
            var postHide = view as IPostHide;
            if (postHide != null)
                seq.AddAction(() => postHide.OnPostHide(options));
        }

        private void PrepareContainers(Presentation presentation)
        {
            // Ensure parent has not been destroyed in the meantime
            if (Container1 == null)
                throw new TransitionParentDestroyedException();

            // Lazily initialize containers
            _sourceContainer = _sourceContainer ?? Container1;
            _targetContainer = _targetContainer ?? Container2;

            // Swap containers
            var temp = _targetContainer;
            _targetContainer = _sourceContainer;
            _sourceContainer = temp;

            var direction = presentation.Options.GetDirectionOrDefault();
            var targetView = presentation.TargetView;
            var transition = presentation.Transition;

            transition.Prepare(_sourceContainer, _targetContainer, direction);
            ReplaceView(_targetContainer, targetView);

            _sourceContainer.SetActive(true);
            _targetContainer.SetActive(true);
        }

        protected void CompleteTransition(Presentation presentation)
        {
            var targetView = presentation.TargetView;
            var transition = presentation.Transition;

            _view.Value = targetView;
            RemoveAllViews(_sourceContainer);
            _sourceContainer.SetActive(false);

            transition.Complete(_sourceContainer, _targetContainer);
        }

        #endregion
    }
}