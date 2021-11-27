using System;
using UniRx;

namespace Silphid.Showzup
{
    public class OptionsDecoratorPresenter : IPresenter
    {
        private readonly IPresenter _inner;
        private readonly Func<IOptions, IOptions> _optionsSelector;
        private readonly Func<object, IOptions, IOptions> _inputOptionsSelector;

        public OptionsDecoratorPresenter(IPresenter inner, Func<IOptions, IOptions> optionsSelector)
        {
            _inner = inner;
            _optionsSelector = optionsSelector;
        }

        public OptionsDecoratorPresenter(IPresenter inner, Func<object, IOptions, IOptions> inputOptionsSelector)
        {
            _inner = inner;
            _inputOptionsSelector = inputOptionsSelector;
        }

        public IObservable<IView> Present(object input, IOptions options = null) =>
            _inner.Present(
                input,
                _optionsSelector != null
                    ? _optionsSelector(options)
                    : _inputOptionsSelector(input, options));

        public IReadOnlyReactiveProperty<PresenterState> State => _inner.State;
    }
}