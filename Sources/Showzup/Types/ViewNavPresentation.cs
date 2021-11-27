namespace Silphid.Showzup
{
    public class ViewNavPresentation<TView> : NavPresentation where TView : IView
    {
        public TView View { get; }

        public ViewNavPresentation(NavPresentation navPresentation, TView view)
            : base(
                navPresentation.Source,
                navPresentation.Target,
                navPresentation.Options,
                navPresentation.Transition,
                navPresentation.Duration,
                navPresentation.Parallel)
        {
            View = view;
        }
    }
}