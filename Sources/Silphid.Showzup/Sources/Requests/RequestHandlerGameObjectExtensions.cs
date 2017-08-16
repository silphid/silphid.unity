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
    }
}