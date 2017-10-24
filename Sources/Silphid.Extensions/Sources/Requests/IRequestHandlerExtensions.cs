using System;

namespace Silphid.Requests
{
    public static class IRequestHandlerExtensions
    {
        public static bool Handle<TRequest>(this IRequestHandler This) where TRequest : IRequest, new() =>
            This.Handle(new TRequest());
        
        public static void Handle(this IRequestHandler requestHandler, Exception exception)
        {
            requestHandler.Handle(exception as IRequest ?? new RequestException(exception));
        }
    }
}