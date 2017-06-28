using System;
using System.Collections.Generic;
using System.Linq;

namespace Silphid.Showzup.Injection
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
            ?? Injection.Container.Null;

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

        public IBinding With(IResolver resolver)
        {
            throw new NotImplementedException();
        }

        public IBinding With(Action<IBinder> bind)
        {
            _bindings.ForEach(x => x.IntoList());
            return this;
        }
    }
}