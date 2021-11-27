using System;

namespace Silphid.Requests
{
    public static class IRequestHandlerExtensions
    {
        public static void Handle<TRequest>(this IRequestHandler This) where TRequest : IRequest, new() =>
            This.Handle(new TRequest());

        public static void Handle(this IRequestHandler requestHandler, Exception exception) =>
            requestHandler.Handle(exception.AsRequest());
    }
}