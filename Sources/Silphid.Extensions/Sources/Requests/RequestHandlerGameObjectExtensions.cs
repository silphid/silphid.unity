using System;
using System.Linq;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Requests
{
    public static class RequestHandlerGameObjectExtensions
    {
        public static bool Send(this GameObject This, IRequest request) =>
            This.SelfAndAncestors<IRequestHandler>()
                .Select(x => x.Handle(request))
                .FirstOrDefault(x => x);

        public static bool Send(this Component This, IRequest request) =>
            This.gameObject.Send(request);

        public static bool Send<TRequest>(this Component This) where TRequest : IRequest, new() =>
            This.Send(new TRequest());

        public static bool Send(this GameObject This, Exception exception) =>
            This.Send(exception as IRequest ?? new RequestException(null, exception));

        public static bool Send(this Component This, Exception exception) =>
            This.gameObject.Send(exception);
    }
}