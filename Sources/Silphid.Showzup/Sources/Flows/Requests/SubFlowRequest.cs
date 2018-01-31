namespace Silphid.Showzup.Flows
{
    public interface ISubFlowRequest : IFlowRequest
    {
    }
    
    public abstract class SubFlowRequest<TFlow> : FlowRequest<TFlow>, ISubFlowRequest
    {
        protected SubFlowRequest(params object[] parameters) :
            base(parameters)
        {
        }
    }
}