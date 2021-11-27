using Silphid.Requests;

namespace Silphid.Showzup.Test.Controls.Virtual
{
    public class AddAfterRequest : IRequest
    {
        public object Model { get; }

        public AddAfterRequest(object model)
        {
            Model = model;
        }
    }
}