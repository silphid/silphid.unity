using System;

namespace Silphid.Requests
{
    public interface ITypedRequestHandler : IRequestHandler
    {
        Type SupportedRequestType { get; }
    }
}