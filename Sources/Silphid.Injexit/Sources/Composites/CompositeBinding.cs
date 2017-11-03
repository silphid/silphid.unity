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

        public IBinding Alias(Type aliasAbstractionType)
        {
            throw new NotSupportedException("Alias not supported on CompositeBinding");
        }

        public IBinding Using(IResolver resolver)
        {
            throw new NotSupportedException("Using not supported on CompositeBinding");
        }

        public IBinding UsingRecursively(IResolver resolver)
        {
            throw new NotSupportedException("UsingRecursively not supported on CompositeBinding");
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
    }
}