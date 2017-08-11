using System;
using System.Linq;
using Silphid.Extensions;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup.Requests
{
    public static class RequestHandlerGameObjectExtensions
    {
        public static bool CanHandle(this GameObject This, IRequest request) =>
            This.SelfAndAncestors<IRequestHandler>()
                .Any(x => x.CanHandle(request));

        public static IObservable<Unit> Handle(this GameObject This, IRequest request) =>
            This.SelfAndAncestors<IRequestHandler>()
                .FirstOrDefault()
                ?.Handle(request);
    }
}