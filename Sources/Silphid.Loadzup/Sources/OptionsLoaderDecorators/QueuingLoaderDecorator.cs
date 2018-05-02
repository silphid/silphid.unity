namespace Silphid.Loadzup
{
    public class QueuingLoaderDecorator : OptionsLoaderDecoratorBase
    {
        private readonly bool _isPriority;

        public QueuingLoaderDecorator(ILoader loader, bool isPriority) : base(loader)
        {
            _isPriority = isPriority;
        }

        protected override void UpdateOptions(Options options)
        {
            options.IsPriority = _isPriority;
        }
    }
}