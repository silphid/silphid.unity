using System;
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

        [Inject, Optional] public ITransitionResolver TransitionResolver { get; set; }

        #endregion

        #region Virtual and abstract overrides

        protected override ICompletable Present(Presentation presentation)
        {
            return PerformTransition(presentation);
        }

        #endregion

        #region Implementation

        protected ITransition ResolveTransition(Presentation presentation) =>
            presentation.Options.GetTransition() ?? TransitionResolver?.Resolve(presentation) ??
            DefaultTransition ?? InstantTransition.Instance;

        protected float ResolveDuration(ITransition transition, IOptions options) =>
            options.GetTransitionDuration() ?? transition.Duration;

        private ICompletable PerformTransition(Presentation presentation)
        {
            var sourceView = presentation.SourceView;
            var targetView = presentation.TargetView;
            var options = presentation.Options;
            var transition = presentation.Transition = ResolveTransition(presentation);
            var duration = presentation.Duration = ResolveDuration(transition, options);

            if (!gameObject.activeInHierarchy || sourceView == null && TransitionInstantlyFromNull ||
                targetView == null && TransitionInstantlyToNull)
            {
                transition = InstantTransition.Instance;
                duration = 0f;
            }

            return Sequence.Create(
                                seq =>
                                {
                                    seq.AddAction(() => PrepareContainers(presentation));

                                    PreHide(sourceView, options, seq);
                                    Deconstruct(sourceView, options, seq);
                                    PreShow(targetView, options, seq);

                                    seq.Add(() => Observable.NextFrame());
                                    seq.Add(
                                        () => transition.Perform(
                                            _sourceContainer,
                                            _targetContainer,
                                            options,
                                            duration));

                                    PostHide(sourceView, options, seq);
                                    Construct(targetView, options, seq);
                                    PostShow(targetView, options, seq);
                                })
                           .DoOnError(
                                ex =>
                                {
                                    if (ex is TransitionParentDestroyedException)
                                        Log.Error("TransitionControl has been destroyed");
                                    else if (targetView != null && targetView.GameObject != null)
                                        targetView.GameObject.Destroy();

                                    Log.Error(
                                        new Exception($"Failed to transition from {sourceView} to {targetView}.", ex));
                                })
                           .Finally(() => CompleteTransition(presentation));
        }

        private void PreHide(IView view, IOptions options, ISequencer seq)
        {
            if (view is IPreHide preHide)
                seq.AddAction(() => preHide.OnPreHide());
        }

        private void PreShow(IView view, IOptions options, ISequencer seq)
        {
            if (view is IPreShow preShow)
                seq.AddAction(() => preShow.OnPreShow());
        }

        private void Deconstruct(IView view, IOptions options, ISequencer seq)
        {
            if (view is IDeconstructable deconstructable)
                seq.Add(() => deconstructable.Deconstruct(options));
        }

        private void Construct(IView view, IOptions options, ISequencer seq)
        {
            if (view is IConstructable constructable)
                seq.Add(() => constructable.Construct(options));
        }

        private void PostShow(IView view, IOptions options, ISequencer seq)
        {
            if (view is IPostShow postShow)
                seq.AddAction(() => postShow.OnPostShow());
        }

        private void PostHide(IView view, IOptions options, ISequencer seq)
        {
            if (view is IPostHide postHide)
                seq.AddAction(() => postHide.OnPostHide());
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

            var targetView = presentation.TargetView;
            var transition = presentation.Transition;

            transition.Prepare(_sourceContainer, _targetContainer, presentation.Options);
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