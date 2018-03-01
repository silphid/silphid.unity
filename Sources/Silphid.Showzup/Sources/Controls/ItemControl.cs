using System;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public class ItemControl : SinglePresenterControl
    {
        public GameObject Container;

        protected override ICompletable Present(Presentation presentation)
        {
            if (Container == null)
                throw new InvalidOperationException($"Must specify ContentContainer property of ContentControl {gameObject}");

            var sourceView = presentation.SourceView;
            var targetView = presentation.TargetView;
            var options = presentation.Options;

            PreHide(sourceView, options);
            PreShow(targetView, options);

            ReplaceView(Container, targetView);
            _view.Value = targetView;

            PostHide(sourceView, options);
            PostShow(targetView, options);

            return Completable.Empty();
        }

        private void PreHide(IView view, Options options)
        {
            (view as IPreHide)?.OnPreHide(options);
        }

        private void PreShow(IView view, Options options)
        {
            (view as IPreShow)?.OnPreShow(options);
        }

        private void PostShow(IView view, Options options)
        {
            (view as IPostShow)?.OnPostShow(options);
        }

        private void PostHide(IView view, Options options)
        {
            (view as IPostHide)?.OnPostHide(options);
        }
    }
}