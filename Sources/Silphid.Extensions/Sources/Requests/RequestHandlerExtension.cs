using System;
using Silphid.Requests;

namespace Silphid.Requests
{
    public static class RequestHandlerExtension
    {
        public static void Handle(this IRequestHandler requestHandler, Exception exception)
        {
            requestHandler.Handle(exception as IRequest ?? new RequestException(null, exception));
        }
    }
}