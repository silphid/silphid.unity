using System;
using Silphid.Requests;

namespace Silphid.Showzup.Flows
{
    public interface IFlowRequest : IRequest
    {
        Type FlowType { get; }
        object[] Parameters { get; }
    }
    
    public abstract class FlowRequest<TFlow> : Request, IFlowRequest
    {
        public Type FlowType => typeof(TFlow);
        public object[] Parameters { get; }

        protected FlowRequest(params object[] parameters)
        {
            Parameters = parameters;
        }
    }
}