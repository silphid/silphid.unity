using System;
using Silphid.Extensions;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public class ItemControl : SinglePresenterControl
    {
        public GameObject Container;
        public bool AutoSelect = true;

        protected override IObservable<Unit> Present(Presentation presentation)
        {
            if (Container == null)
                throw new InvalidOperationException($"Must specify ContentContainer property of ContentControl {gameObject}");

            var sourceView = presentation.SourceView;
            var targetView = presentation.TargetView;
            var options = presentation.Options;

            PreHide(sourceView, options);
            PreShow(targetView, options);

            ReplaceView(Container, targetView);
            if (targetView != null)
                targetView.IsActive = true;
            _view.Value = targetView;

            PostHide(sourceView, options);
            PostShow(targetView, options);

            return Observable.ReturnUnit();
        }

        protected virtual void Start()
        {
            if (AutoSelect)
                _view
                    .CombineLatest(IsSelected.WhereTrue(), (x, y) => x)
                    .Subscribe(SelectView);
        }

        protected virtual void SelectView(IView view)
        {
            view.GameObject.SelectDeferred();
        }

        private void PreHide(IView view, Options options)
        {
            var preHide = view as IPreHide;
            if (preHide != null)
                preHide.OnPreHide(options);
        }

        private void PreShow(IView view, Options options)
        {
            var preShow = view as IPreShow;
            if (preShow != null)
                preShow.OnPreShow(options);
        }

        private void PostShow(IView view, Options options)
        {
            var postShow = view as IPostShow;
            if (postShow != null)
                postShow.OnPostShow(options);
        }

        private void PostHide(IView view, Options options)
        {
            var postHide = view as IPostHide;
            if (postHide != null)
                postHide.OnPostHide(options);
        }
    }
}