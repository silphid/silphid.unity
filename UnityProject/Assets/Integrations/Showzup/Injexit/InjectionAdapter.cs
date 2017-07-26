using System;
using System.Collections.Generic;
using Silphid.Injexit;

namespace Silphid.Showzup
{
    public class InjectionAdapter : IInjectionAdapter
    {
        private readonly IContainer _container;

        public InjectionAdapter(IContainer container)
        {
            _container = container;
        }

        public object Resolve(Type type, IEnumerable<object> extraInstances = null) =>
            extraInstances == null
                ? _container.Resolve(type)
                : _container
                    .Using(x => x.BindInstances(extraInstances))
                    .Resolve(type);

        public void Inject(object obj, IEnumerable<object> extraInstances = null)
        {
            if (extraInstances == null)
                _container.Inject(obj);
            else
                _container
                    .Using(x => x.BindInstances(extraInstances))
                    .Inject(obj);
        }
    }
}