namespace Silphid.Showzup.Requests
{
    public interface IRequestHandler
    {
        /// <returns>Whether it handled the request or not.</returns>
        bool Handle(IRequest request);
    }
}