using System;

namespace Silphid.Showzup
{
    public class TransitionSelectorPresenterDecorator : OptionsPresenterDecoratorBase
    {
        private readonly Func<object, Options, ITransition> _selector;
        
        public TransitionSelectorPresenterDecorator(IPresenter presenter, Func<object, Options, ITransition> selector) : base(presenter)
        {
            _selector = selector;
        }

        protected override void UpdateOptions(object input, Options options)
        {
            var transition = _selector(input, options);
            if (transition != null)
                options.Transition = transition;
        }
    }
}