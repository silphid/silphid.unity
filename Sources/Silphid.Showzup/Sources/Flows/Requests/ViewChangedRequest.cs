using Silphid.Requests;

namespace Silphid.Showzup.Flows
{
    public class ViewChangedRequest : Request
    {
        public IView View { get; }

        public ViewChangedRequest(IView view)
        {
            View = view;
        }
    }
}