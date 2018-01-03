using System;
using UniRx;

namespace Silphid.Showzup
{
    public abstract class OptionsPresenterDecoratorBase : IPresenter
    {
        private readonly IPresenter _presenter;

        protected OptionsPresenterDecoratorBase(IPresenter presenter)
        {
            _presenter = presenter;
        }

        protected abstract void UpdateOptions(object input, Options options);

        private Options GetOptions(object input, Options options)
        {
            if (options == null)
                options = new Options();
            
            UpdateOptions(input, options);
            return options;
        }

        #region IPresenter members

        public IObservable<IView> Present(object input, Options options = null) =>
            _presenter.Present(input, GetOptions(input, options));

        public IReadOnlyReactiveProperty<PresenterState> State => _presenter.State;

        #endregion
    }
}