namespace Silphid.Showzup.Flows
{
    public interface ITopFlowRequest : IFlowRequest {}

    public abstract class TopFlowRequest<TFlow> : FlowRequest<TFlow>, ITopFlowRequest
    {
        protected TopFlowRequest(IFlowOptions options)
            : base(options) {}
    }
}