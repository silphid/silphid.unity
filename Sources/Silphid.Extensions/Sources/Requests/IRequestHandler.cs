namespace Silphid.Requests
{
    public interface IRequestHandler
    {
        /// <returns>Returns null if the incoming request was handled OR
        /// same request if it still needs to be handled OR
        /// potentially a new request to handle instead.</returns>
        IRequest Handle(IRequest request);
    }
}