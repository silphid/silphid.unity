namespace Silphid.Requests
{
    /// <summary>
    /// An object that allows to send requests. This would typically be injected into a request handler that needs
    /// to send requests to other request handlers.
    /// </summary>
    public interface IRequestSink
    {
        void Send(IRequest request);
    }
}