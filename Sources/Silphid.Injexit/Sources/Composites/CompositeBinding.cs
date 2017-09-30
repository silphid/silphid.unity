using System;
using System.Collections.Generic;
using System.Linq;

namespace Silphid.Injexit
{
    public class CompositeBinding : IBinding
    {
        private readonly List<IBinding> _bindings;
        
        public CompositeBinding(IEnumerable<IBinding> bindings)
        {
            _bindings = bindings.ToList();
        }

        public IContainer Container =>
            _bindings.FirstOrDefault()?.Container
            ?? Injexit.Container.Null;

        public IBinding AsList()
        {
            _bindings.ForEach(x => x.AsList());
            return this;
        }

        public IBinding AsSingle()
        {
            _bindings.ForEach(x => x.AsList());
            return this;
        }

        public IBinding AsEagerSingle()
        {
            _bindings.ForEach(x => x.AsEagerSingle());
            return this;
        }

        public IBinding Using(IResolver resolver)
        {
            throw new NotImplementedException();
        }

        public IBinding WithId(string id)
        {
            _bindings.ForEach(x => x.WithId(id));
            return this;
        }

        public IBinding AsAlias(string alias)
        {
            throw new NotImplementedException();
        }

        public IBinding Using(Action<IBinder> bind)
        {
            _bindings.ForEach(x => x.AsList());
            return this;
        }
    }
}