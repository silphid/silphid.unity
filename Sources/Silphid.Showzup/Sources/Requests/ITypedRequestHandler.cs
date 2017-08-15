using System;

namespace Silphid.Showzup.Requests
{
    public interface ITypedRequestHandler : IRequestHandler
    {
        Type SupportedRequestType { get; }
    }
}