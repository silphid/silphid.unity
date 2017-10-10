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
        
        private readonly List<ITypedRequestHandler> _typedRequestHandlers;

        public CompositeTypedRequestHandler(
            ITypedRequestHandler[] typedRequestHandlers)
        {
            _typedRequestHandlers = typedRequestHandlers
                .OrderByDescending(x => GetInheritanceDepth(x.SupportedRequestType))
                .ToList();
        }

        private int GetInheritanceDepth(Type type)
        {
            int depth;
            for (depth = -1; type != null; depth++)
                type = type.GetBaseType();

            return depth;
        }

        public bool Handle(IRequest request)
        {
            Log.Debug($"CompositeTypedRequestHandler received {request}");

            var requestType = request.GetType();
            var handlers = _typedRequestHandlers.Where(x => x.SupportedRequestType.IsAssignableFrom(requestType));
            foreach (var handler in handlers)
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