using Silphid.Requests;

namespace Silphid.Showzup.Requests
{
    public class SelectRequest : Request
    {
        public object Input { get; }

        public SelectRequest(object input)
        {
            Input = input;
        }
    }
}