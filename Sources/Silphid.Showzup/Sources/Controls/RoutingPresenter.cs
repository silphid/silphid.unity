using System;

namespace Silphid.Showzup
{
    public class RoutingPresenter : PresenterControl
    {
        public override IObservable<IView> Present(object input, Options options = null)
        {
            throw new NotSupportedException($"Remove RoutingPresenter from game object: {gameObject}");            
        }

        internal void Start()
        {
            throw new NotSupportedException($"Remove RoutingPresenter from game object: {gameObject}");
        }
    }
}