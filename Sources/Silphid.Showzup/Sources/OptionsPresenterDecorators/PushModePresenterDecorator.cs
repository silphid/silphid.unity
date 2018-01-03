namespace Silphid.Showzup
{
    internal class PushModePresenterDecorator : OptionsPresenterDecoratorBase
    {
        private readonly PushMode _pushMode;

        public PushModePresenterDecorator(IPresenter presenter, PushMode pushMode) : base(presenter)
        {
            _pushMode = pushMode;
        }

        protected override void UpdateOptions(object input, Options options)
        {
            options.PushMode = _pushMode;
        }
    }
}