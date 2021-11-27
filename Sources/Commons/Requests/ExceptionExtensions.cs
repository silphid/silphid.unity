using System;

namespace Silphid.Requests
{
    public static class ExceptionExtensions
    {
        public static IRequest AsRequest(this Exception This) =>
            This is IRequest
                ? (IRequest) This
                : new ExceptionRequest(This);
    }
}