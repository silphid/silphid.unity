using System;
using System.Collections.Generic;

namespace Silphid.Showzup
{
    public interface IInjectionAdapter
    {
        object Resolve(Type type, IEnumerable<object> extraInstances = null);
        void Inject(object obj, IEnumerable<object> extraInstances = null);
    }
}