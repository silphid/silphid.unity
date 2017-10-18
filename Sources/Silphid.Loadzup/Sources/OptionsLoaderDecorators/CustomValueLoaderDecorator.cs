namespace Silphid.Loadzup
{
    public class CustomValueLoaderDecorator : OptionsLoaderDecoratorBase
    {
        private readonly object _key;
        private readonly object _value;

        public CustomValueLoaderDecorator(ILoader loader, object key, object value) : base(loader)
        {
            _key = key;
            _value = value;
        }

        protected override void UpdateOptions(Options options)
        {
            options.SetCustomValue(_key, _value);
        }
    }
}