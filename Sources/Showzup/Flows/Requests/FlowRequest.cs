using System;
using Silphid.Requests;

namespace Silphid.Showzup.Flows
{
    public interface IFlowRequest : IRequest
    {
        Type FlowType { get; }
        IFlowOptions Options { get; }
    }

    public abstract class FlowRequest<TFlow> : Request, IFlowRequest
    {
        public Type FlowType => typeof(TFlow);
        public IFlowOptions Options { get; }

        protected FlowRequest(IFlowOptions options = null)
        {
            Options = options;
        }
    }
}