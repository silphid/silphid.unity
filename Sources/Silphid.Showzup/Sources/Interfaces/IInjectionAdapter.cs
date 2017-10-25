using System;
using System.Collections.Generic;

namespace Silphid.Showzup
{
    public interface IInjectionAdapter
    {
        object Resolve(Type type, IDictionary<Type, object> parameters = null);
        void Inject(object obj, IDictionary<Type, object> parameters = null);
    }
}