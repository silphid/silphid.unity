using Silphid.Requests;

namespace Silphid.Showzup.Test.Controls.Virtual
{
    public class RemoveRequest : IRequest
    {
        public object Model { get; }

        public RemoveRequest(object model)
        {
            Model = model;
        }
    }
}