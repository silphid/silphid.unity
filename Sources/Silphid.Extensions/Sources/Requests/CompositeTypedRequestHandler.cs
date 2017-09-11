using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Requests
{
    public class CompositeTypedRequestHandler : IRequestHandler
    {
        private readonly Dictionary<Type, ITypedRequestHandler> _typedRequestHandlers;
        private readonly ILogger _logger;

        public CompositeTypedRequestHandler(
            ITypedRequestHandler[] typedRequestHandlers,
            ILogger logger = null)
        {
            _typedRequestHandlers = typedRequestHandlers.ToDictionary(x => x.SupportedRequestType);
            _logger = logger;
        }

        public IRequest Handle(IRequest request)
        {
            while (true)
            {
                _logger?.Log($"CompositeTypedRequestHandler received {request}");

                var handler = _typedRequestHandlers.GetValueOrDefault(request.GetType());
                if (handler != null)
                {
                    _logger?.Log($"Trying to handle {request} with {handler}");
                    var outRequest = handler.Handle(request);
                    
                    // Request handled successfully
                    if (outRequest == null)
                    {
                        _logger?.Log($"{request} was handled successfully by {handler}");
                        return null;
                    }

                    // Handler returned new request to handle?
                    if (outRequest != request)
                    {
                        _logger?.Log($"{handler} returned new {request}");
                        request = outRequest;
                        continue;
                    }
                }

                return request;
            }
        }
    }
}