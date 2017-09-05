using System;

namespace Silphid.Showzup
{
    public abstract class OptionsPresenterDecoratorBase : IPresenter
    {
        private readonly IPresenter _presenter;

        protected OptionsPresenterDecoratorBase(IPresenter presenter)
        {
            _presenter = presenter;
        }

        protected abstract void UpdateOptions(Options options);

        private Options GetOptions(Options options)
        {
            if (options == null)
                options = new Options();
            
            UpdateOptions(options);
            return options;
        }

        public IObservable<IView> Present(object input, Options options = null) =>
            _presenter.Present(input, GetOptions(options));
    }
}