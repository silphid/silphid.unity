using Silphid.Requests;

namespace Silphid.Showzup.Requests
{
    public class PresentRequest : Request
    {
        public object Input { get; }
        public IOptions Options { get; }

        public PresentRequest(object input, IOptions options = null)
        {
            Input = input;
            Options = options;
        }
    }
}