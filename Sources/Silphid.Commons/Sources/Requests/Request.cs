namespace Silphid.Requests
{
    public abstract class Request : IRequest
    {
        public override string ToString() => GetType().Name;
    }
}