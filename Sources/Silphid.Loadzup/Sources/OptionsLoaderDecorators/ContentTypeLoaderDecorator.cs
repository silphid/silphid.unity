namespace Silphid.Loadzup
{
    public class ContentTypeLoaderDecorator : OptionsLoaderDecoratorBase
    {
        private readonly ContentType _contentType;

        public ContentTypeLoaderDecorator(ILoader loader, ContentType contentType) : base(loader)
        {
            _contentType = contentType;
        }

        protected override void UpdateOptions(Options options)
        {
            if (options.ContentType == null && _contentType != null)
                options.ContentType = _contentType;
        }
    }
}