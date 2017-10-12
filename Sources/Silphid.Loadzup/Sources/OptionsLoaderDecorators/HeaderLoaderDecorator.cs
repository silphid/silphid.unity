namespace Silphid.Loadzup
{
    public class HeaderLoaderDecorator : OptionsLoaderDecoratorBase
    {
        private readonly string _key;
        private readonly string _value;

        public HeaderLoaderDecorator(ILoader loader, string key, string value) : base(loader)
        {
            _key = key;
            _value = value;
        }

        protected override void UpdateOptions(Options options)
        {
            options.SetHeader(_key, _value);
        }
    }
}