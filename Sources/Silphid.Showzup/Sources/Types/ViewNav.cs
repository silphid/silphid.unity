namespace Silphid.Showzup
{
    public class ViewNav<TView> : Nav where TView : IView
    {
        public TView View { get; }

        public ViewNav(Nav nav, TView view) : base(nav.Source, nav.Target, nav.Parallel, nav.Transition, nav.Duration)
        {
            View = view;
        }
    }
}