using System.Linq;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup.Requests
{
    public static class RequestHandlerGameObjectExtensions
    {
        public static bool Handle(this GameObject This, IRequest request) =>
            This.SelfAndAncestors<IRequestHandler>()
                .Select(x => x.Handle(request))
                .FirstOrDefault(x => x);

        public static bool Handle(this Component This, IRequest request) =>
            This.gameObject.Handle(request);

        public static bool Handle<TRequest>(this Component This) where TRequest : IRequest, new() =>
            This.Handle(new TRequest());
    }
}