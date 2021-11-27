namespace Silphid.Showzup.Flows
{
    public interface ISubFlowRequest : IFlowRequest {}

    public abstract class SubFlowRequest<TFlow> : FlowRequest<TFlow>, ISubFlowRequest
    {
        protected SubFlowRequest(IFlowOptions options)
            : base(options) {}
    }
}