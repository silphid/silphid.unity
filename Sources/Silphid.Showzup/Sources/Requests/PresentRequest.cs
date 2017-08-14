namespace Silphid.Showzup.Requests
{
    public class PresentRequest : IRequest
    {
        public object Input { get; }
        public Options Options { get; }

        public PresentRequest(object input, Options options)
        {
            Input = input;
            Options = options;
        }
    }
}