using System;

namespace Silphid.Requests
{
    public abstract class TypedRequestHandler<TRequest> : ITypedRequestHandler where TRequest : class, IRequest
    {
        public Type SupportedRequestType => typeof(TRequest);
        
        public bool Handle(IRequest request)
        {
            var req = request as TRequest;
            if (req != null)
                return Handle(req);

            return false;
        }

        protected abstract bool Handle(TRequest request);
    }
}