using System;

namespace Silphid.Loadzup
{
    public class TimeoutLoaderDecorator : OptionsLoaderDecoratorBase
    {
        private readonly TimeSpan _timeout;
        
        public TimeoutLoaderDecorator(ILoader loader, TimeSpan timeout) : base(loader)
        {
            _timeout = timeout;
        }

        protected override void UpdateOptions(Options options)
        {
            options.Timeout = _timeout;
        }
    }
}