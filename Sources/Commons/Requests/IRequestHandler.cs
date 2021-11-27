namespace Silphid.Requests
{
    public interface IRequestHandler
    {
        /// <returns>Whether request was fully handled or not.</returns>
        bool Handle(IRequest request);
    }
}