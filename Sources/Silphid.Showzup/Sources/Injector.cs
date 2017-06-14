using System;

namespace Silphid.Showzup
{
    public class Injector : IInjector
    {
        private readonly Action<object> action;

        public Injector(Action<object> action)
        {
            this.action = action;
        }

        public void Inject(object obj)
        {
            action(obj);
        }
    }
}