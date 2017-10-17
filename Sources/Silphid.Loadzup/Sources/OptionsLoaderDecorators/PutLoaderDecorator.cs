namespace Silphid.Loadzup
{
    public class PutLoaderDecorator : OptionsLoaderDecoratorBase
    {
        private readonly string _body;
        
        public PutLoaderDecorator(ILoader loader, string body) : base(loader)
        {
            _body = body;
        }

        protected override void UpdateOptions(Options options)
        {
            options.Method = HttpMethod.Put;
            options.PutBody = _body;
        }
    }
}