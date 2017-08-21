using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silphid.Extensions;

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

        public IBinding Add<TConcretion>() where TConcretion : TAbstraction
        {
            var binding = _binder.Bind<TAbstraction, TConcretion>().AsList();
            _bindings.Add(binding);
            return binding;
        }

        public IBinding AddInstance<TConcretion>(TConcretion instance) where TConcretion : TAbstraction
        {
            var binding = _binder.BindInstance<TAbstraction>(instance).AsList();
            _bindings.Add(binding);
            return binding;
        }

        public void BindAll(Assembly assembly = null)
        {
            var types = (assembly ?? typeof(TAbstraction).Assembly).GetTypes();
            types
                .Where(x => !x.IsAbstract && !x.ContainsGenericParameters && x.IsAssignableTo<TAbstraction>())
                .ForEach(x => _bindings.Add(_binder.Bind<TAbstraction>(x).AsList()));
        }
    }
}