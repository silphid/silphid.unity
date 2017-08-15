using System;
using UniRx;

namespace Silphid.Showzup.Requests
{
    public abstract class TypedRequestHandler<TRequest> : ITypedRequestHandler where TRequest : class, IRequest
    {
        public Type SupportedRequestType => typeof(TRequest);
        
        public IObservable<Unit> Handle(IRequest request)
        {
            var req = request as TRequest;
            return req != null
                ? Handle(req)
                : null;
        }

        protected abstract IObservable<Unit> Handle(TRequest request);
    }
}