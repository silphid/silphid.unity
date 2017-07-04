using System;
using System.Linq;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup
{
    public static class RoutingPresenterGameObjectExtensions
    {
        public static IObservable<IView> RoutePresent(this GameObject This, object input, Options options = null) =>
            This.SelfAndAncestors<RoutingPresenter>()
                .First()
                .Present(input, options);
    }
}