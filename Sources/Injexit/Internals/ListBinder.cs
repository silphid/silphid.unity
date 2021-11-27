using System;
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

        public IBinding<TAbstraction> Add<TConcretion>() where TConcretion : TAbstraction
        {
            var binding = _binder.Bind<TAbstraction, TConcretion>()
                                 .InList();
            _bindings.Add(binding);
            return binding;
        }

        public IBinding<TAbstraction> AddInstance<TConcretion>(TConcretion instance) where TConcretion : TAbstraction
        {
            var binding = _binder.BindInstance<TAbstraction>(instance)
                                 .InList();
            _bindings.Add(binding);
            return binding;
        }

        public IBinding<TAbstraction> AddReference(BindingId id)
        {
            var binding = _binder.BindReference<TAbstraction>(id)
                                 .InList();
            _bindings.Add(binding);
            return binding;
        }

        public void BindAll(Assembly assembly = null, Func<Type, bool> exclude = null)
        {
            BindAll(new[] { assembly }, exclude);
        }

        public void BindAll(Assembly[] assemblies = null, Func<Type, bool> exclude = null)
        {
            var types = assemblies != null
                            ? assemblies.SelectMany(x => x.GetTypes())
                                        .ToArray()
                            : typeof(TAbstraction).Assembly.GetTypes();

            types.Where(
                      x => !x.IsAbstract && !x.ContainsGenericParameters && x.IsAssignableTo<TAbstraction>() &&
                           exclude?.Invoke(x) != true)
                 .ForEach(
                      x => _bindings.Add(
                          _binder.Bind<TAbstraction>(x)
                                 .InList()));
        }
    }
}