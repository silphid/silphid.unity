using System;

namespace Silphid.Requests
{
    public static class IRequestSinkExtensions
    {
        public static void Send<TRequest>(this IRequestSink This) where TRequest : IRequest, new() =>
            This.Send(new TRequest());

        public static void Send(this IRequestSink This, Exception exception) =>
            This.Send(exception.AsRequest());
    }
}