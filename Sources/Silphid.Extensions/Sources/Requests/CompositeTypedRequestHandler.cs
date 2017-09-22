using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Silphid.Extensions;

namespace Silphid.Requests
{
    public class CompositeTypedRequestHandler : IRequestHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CompositeTypedRequestHandler));
        
        private readonly Dictionary<Type, ITypedRequestHandler> _typedRequestHandlers;

        public CompositeTypedRequestHandler(
            ITypedRequestHandler[] typedRequestHandlers)
        {
            _typedRequestHandlers = typedRequestHandlers.ToDictionary(x => x.SupportedRequestType);
        }

        public bool Handle(IRequest request)
        {
            Log.Debug($"CompositeTypedRequestHandler received {request}");

            var handler = _typedRequestHandlers.GetValueOrDefault(request.GetType());
            if (handler != null)
            {
                Log.Debug($"Trying to handle {request} with {handler}");
                if (handler.Handle(request))
                {
                    Log.Debug($"{request} was handled successfully by {handler}");
                    return true;
                }
            }

            return false;
        }
    }
}