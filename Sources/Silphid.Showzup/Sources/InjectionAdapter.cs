using System;
using System.Collections.Generic;

namespace Silphid.Showzup
{
    public class InjectionAdapter : IInjectionAdapter
    {
        private readonly Func<Type, IEnumerable<object>, object> resolve;
        private readonly Action<object, IEnumerable<object>> inject;

        public InjectionAdapter(
            Func<Type, IEnumerable<object>, object> resolve,
            Action<object, IEnumerable<object>> inject)
        {
            this.resolve = resolve;
            this.inject = inject;
        }

        public object Resolve(Type type, IEnumerable<object> extraInstances) =>
            resolve(type, extraInstances);

        public void Inject(object obj, IEnumerable<object> extraInstances) =>
            inject(obj, extraInstances);
    }
}