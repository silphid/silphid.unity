using System;

namespace Silphid.Showzup.Requests
{
    public abstract class TypedRequestHandler<TRequest> : ITypedRequestHandler where TRequest : class, IRequest
    {
        public Type SupportedRequestType => typeof(TRequest);
        
        public bool Handle(IRequest request)
        {
            var req = request as TRequest;
            return req != null && Handle(req);
        }

        protected abstract bool Handle(TRequest request);
    }
}