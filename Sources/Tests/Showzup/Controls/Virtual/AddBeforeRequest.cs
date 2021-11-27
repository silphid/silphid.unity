using Silphid.Requests;

namespace Silphid.Showzup.Test.Controls.Virtual
{
    public class AddBeforeRequest : IRequest
    {
        public object Model { get; }

        public AddBeforeRequest(object model)
        {
            Model = model;
        }
    }
}