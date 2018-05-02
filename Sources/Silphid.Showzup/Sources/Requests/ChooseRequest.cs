using Silphid.Requests;

namespace Silphid.Showzup.Requests
{
    public class ChooseRequest : Request
    {
        public object Input { get; }

        public ChooseRequest(object input)
        {
            Input = input;
        }
    }
}