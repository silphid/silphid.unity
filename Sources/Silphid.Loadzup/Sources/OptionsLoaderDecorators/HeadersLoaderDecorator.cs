using System.Collections.Generic;

namespace Silphid.Loadzup
{
    public class HeadersLoaderDecorator : OptionsLoaderDecoratorBase
    {
        private readonly Dictionary<string, string> _headers;

        public HeadersLoaderDecorator(ILoader loader, Dictionary<string, string> headers) : base(loader)
        {
            _headers = headers;
        }

        protected override void UpdateOptions(Options options)
        {
            foreach (var entry in _headers)
                options.SetHeader(entry.Key, entry.Value);
        }
    }
}