using System;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public class ItemControl : PresenterControlBase
    {
        public GameObject Container;

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
            targetView.IsActive = true;
            _view.Value = targetView;

            PostHide(sourceView, options);
            PostShow(targetView, options);

            return Observable.ReturnUnit();
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