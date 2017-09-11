using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Requests
{
    public static class RequestHandlerGameObjectExtensions
    {
        public static void Send(this GameObject This, IRequest request)
        {
            foreach (var gameObject in This.SelfAndAncestors())
            {
                foreach (var handler in gameObject.GetComponents<IRequestHandler>())
                {
                    var outRequest = handler.Handle(request);
                    if (outRequest == null)
                        return;

                    if (outRequest != request)
                    {
                        gameObject.Send(outRequest);
                        return;
                    }
                }
            }
        }

        public static void Send(this Component This, IRequest request) =>
            This.gameObject.Send(request);

        public static void Send<TRequest>(this Component This) where TRequest : IRequest, new() =>
            This.Send(new TRequest());
    }
}