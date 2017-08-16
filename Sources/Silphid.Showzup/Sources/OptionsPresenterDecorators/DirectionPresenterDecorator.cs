namespace Silphid.Showzup
{
    internal class DirectionPresenterDecorator : OptionsPresenterDecoratorBase
    {
        private readonly Direction _direction;

        public DirectionPresenterDecorator(IPresenter presenter, Direction direction) : base(presenter)
        {
            _direction = direction;
        }

        protected override void UpdateOptions(Options options)
        {
            options.Direction = _direction;
        }
    }
}