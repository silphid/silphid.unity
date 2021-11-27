using System;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Silphid.Showzup
{
    public class ItemControl : SinglePresenterControl
    {
        [FormerlySerializedAs("Container")] public GameObject Content;

        protected override ICompletable Present(Presentation presentation)
        {
            if (Content == null)
                throw new InvalidOperationException(
                    $"Must specify ContentContainer property of ContentControl {gameObject}");

            var sourceView = presentation.SourceView;
            var targetView = presentation.TargetView;

            PreHide(sourceView);
            PreShow(targetView);

            ReplaceView(Content, targetView);
            _view.Value = targetView;

            PostHide(sourceView);
            PostShow(targetView);

            return Completable.Empty();
        }

        private void PreHide(IView view)
        {
            (view as IPreHide)?.OnPreHide();
        }

        private void PreShow(IView view)
        {
            (view as IPreShow)?.OnPreShow();
        }

        private void PostShow(IView view)
        {
            (view as IPostShow)?.OnPostShow();
        }

        private void PostHide(IView view)
        {
            (view as IPostHide)?.OnPostHide();
        }
    }
}