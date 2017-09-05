namespace Silphid.Showzup
{
    public class TransitionDurationPresenterDecorator : OptionsPresenterDecoratorBase
    {
        private readonly float _duration;

        public TransitionDurationPresenterDecorator(IPresenter presenter, float duration) : base(presenter)
        {
            _duration = duration;
        }

        protected override void UpdateOptions(Options options)
        {
            options.TransitionDuration = _duration;
        }
    }
}