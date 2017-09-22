namespace Silphid.Requests
{
    public static class IRequestHandlerExtensions
    {
        public static bool Handle<TRequest>(this IRequestHandler This) where TRequest : IRequest, new() =>
            This.Handle(new TRequest());
    }
}