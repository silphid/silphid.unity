using System.Collections.Generic;

namespace Silphid.Showzup
{
    internal class ParametersPresenterDecorator : OptionsPresenterDecoratorBase
    {
        private readonly IEnumerable<object> _parameters;

        public ParametersPresenterDecorator(IPresenter presenter, IEnumerable<object> parameters) : base(presenter)
        {
            _parameters = parameters;
        }

        protected override void UpdateOptions(Options options)
        {
            options.Parameters = _parameters;
        }
    }
}