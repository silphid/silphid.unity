namespace Silphid.Loadzup
{
    public class PriorityLoaderDecorator : OptionsLoaderDecoratorBase
    {
        private readonly bool _isPriority;

        public PriorityLoaderDecorator(ILoader loader, bool isPriority) : base(loader)
        {
            _isPriority = isPriority;
        }

        protected override void UpdateOptions(Options options)
        {
            options.IsPriority = _isPriority;
        }
    }
}