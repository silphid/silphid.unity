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

        public IBinding InList()
        {
            _bindings.ForEach(x => x.InList());
            return this;
        }

        public IBinding AsSingle()
        {
            _bindings.ForEach(x => x.InList());
            return this;
        }

        public IBinding AsEagerSingle()
        {
            _bindings.ForEach(x => x.AsEagerSingle());
            return this;
        }

        public IBinding Using(IResolver resolver)
        {
            throw new NotSupportedException();
        }

        public IBinding Named(string name)
        {
            _bindings.ForEach(x => x.Named(name));
            return this;
        }

        public IBinding Id(BindingId id)
        {
            throw new NotSupportedException();
        }

        public IBinding Using(Action<IBinder> bind)
        {
            _bindings.ForEach(x => x.InList());
            return this;
        }
    }
}