namespace Silphid.Showzup
{
    public class TransitionPresenterDecorator : OptionsPresenterDecoratorBase
    {
        private readonly ITransition _transition;
        
        public TransitionPresenterDecorator(IPresenter presenter, ITransition transition) : base(presenter)
        {
            _transition = transition;
        }

        protected override void UpdateOptions(object input, Options options)
        {
            options.Transition = _transition;
        }
    }
}