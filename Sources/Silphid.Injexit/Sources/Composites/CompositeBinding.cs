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

        public IBinding IntoList()
        {
            _bindings.ForEach(x => x.IntoList());
            return this;
        }

        public IBinding AsSingle()
        {
            _bindings.ForEach(x => x.IntoList());
            return this;
        }

        public IBinding Using(IResolver resolver)
        {
            throw new NotImplementedException();
        }

        public IBinding WithId(string id)
        {
            throw new NotImplementedException();
        }

        public IBinding Using(Action<IBinder> bind)
        {
            _bindings.ForEach(x => x.IntoList());
            return this;
        }
    }
}