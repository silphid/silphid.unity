using System;
using System.Collections.Generic;
using Silphid.Injexit;

namespace Silphid.Showzup
{
    /// <summary>
    /// Used by Showzup for all dependency injection (DI) needs. The IInjectionAdapter interface abstracts the DI
    /// framework away from Showzup, so that another DI framework could (in theory) be swapped in, if it
    /// supports all required functionalities.
    /// </summary>
    public class InjectionAdapter : IInjectionAdapter
    {
        private readonly IContainer _container;

        public InjectionAdapter(IContainer container)
        {
            _container = container;
        }

        public object Resolve(Type type, IDictionary<Type, object> parameters = null) =>
            parameters == null
                ? _container.Resolve(type)
                : _container
                    .Using(x => x.BindInstances(parameters))
                    .Resolve(type);

        public void Inject(object obj, IDictionary<Type, object> parameters = null)
        {
            if (parameters == null)
                _container.Inject(obj);
            else
                _container
                    .Using(x => x.BindInstances(parameters))
                    .Inject(obj);
        }
    }
}