using System.Collections.Generic;

namespace Silphid.Injexit
{
    public class ListBinder<TAbstraction> : IListBinder<TAbstraction>
    {
        private readonly IBinder _binder;
        private readonly List<IBinding> _bindings = new List<IBinding>();
        public CompositeBinding CompositeBinding => new CompositeBinding(_bindings);

        public ListBinder(IBinder binder)
        {
            _binder = binder;
        }

        public IBinding Bind<TConcretion>() where TConcretion : TAbstraction
        {
            var binding = _binder.Bind<TAbstraction, TConcretion>().AsList();
            _bindings.Add(binding);
            return binding;
        }

        public IBinding BindInstance<TConcretion>(TConcretion instance) where TConcretion : TAbstraction
        {
            var binding = _binder.BindInstance<TAbstraction>(instance).AsList();
            _bindings.Add(binding);
            return binding;
        }
    }
}