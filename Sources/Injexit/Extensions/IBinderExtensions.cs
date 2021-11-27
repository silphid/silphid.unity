using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silphid.Extensions;

namespace Silphid.Injexit
{
    public static class IBinderExtensions
    {
        #region Bind via factory

        public static IBinding<T> Bind<T>(this IBinder This, Func<IResolver, T> factory) =>
            This.Bind(typeof(T), factory);

        public static IBinding<T> Bind<T>(this IBinder This, Func<T> factory) =>
            This.Bind(typeof(T), _ => factory());

        #endregion

        #region BindInstance(s)

        public static IBinding<T> BindInstance<T>(this IBinder This, T instance) =>
            This.BindInstance(typeof(T), instance)
                .Typed<T>();

        public static IBinding BindInstanceToOwnType(this IBinder This, object instance) =>
            This.BindInstance(instance.GetType(), instance);

        public static IBinding BindInstancesToOwnTypes(this IBinder This, IEnumerable<object> instances) =>
            new CompositeBinding(instances.Select(This.BindInstanceToOwnType));

        public static IBinding BindInstances(this IBinder This, IDictionary<Type, object> instances) =>
            new CompositeBinding(instances.Select(x => This.BindInstance(x.Key, x.Value)));

        #endregion

        #region BindOptionalInstance(s)

        public static IBinding<T> BindOptionalInstance<T>(this IBinder This, T instance) =>
            (instance != null
                 ? This.BindInstance(typeof(T), instance)
                 : Binding.Null).Typed<T>();

        public static IBinding BindOptionalInstance(this IBinder This, object instance) =>
            instance != null
                ? This.BindInstance(instance.GetType(), instance)
                : Binding.Null;

        public static IBinding BindOptionalInstancesToOwnTypes(this IBinder This, IEnumerable<object> instances) =>
            instances != null
                ? new CompositeBinding(
                    instances.WhereNotNull()
                             .Select(This.BindInstanceToOwnType))
                : Binding.Null;

        public static IBinding BindOptionalInstances(this IBinder This, IDictionary<Type, object> instances) =>
            instances != null && instances.Count > 0
                ? new CompositeBinding(
                    instances.Where(x => x.Value != null)
                             .Select(x => This.BindInstance(x.Key, x.Value)))
                : Binding.Null;

        #endregion

        #region Bind

        public static IBinding<TAbstraction> Bind<TAbstraction>(this IBinder This, Type concretionType) =>
            This.Bind(typeof(TAbstraction), concretionType)
                .Typed<TAbstraction>();

        public static IBinding<TAbstraction> Bind<TAbstraction, TConcretion>(this IBinder This)
            where TConcretion : TAbstraction =>
            This.Bind(typeof(TAbstraction), typeof(TConcretion))
                .Typed<TAbstraction>();

        #endregion

        #region BindToSelf

        public static IBinding<T> BindToSelf<T>(this IBinder This) =>
            This.Bind<T, T>();

        public static IBinding<T> BindAnonymous<T>(this IBinder This) =>
            This.BindToSelf<T>();

        public static IBinding<T> BindToSelfAll<T>(this IBinder This,
                                                   Assembly assembly = null,
                                                   Predicate<Type> predicate = null,
                                                   Action<Type, IBinding> action = null) =>
            This.BindToSelfAll<T>(
                assembly != null
                    ? new[] { assembly }
                    : null,
                predicate,
                action);

        public static IBinding<T> BindToSelfAll<T>(this IBinder This,
                                                   Assembly[] assemblies = null,
                                                   Predicate<Type> predicate = null,
                                                   Action<Type, IBinding> action = null) =>
            This.BindAllInternal<T>(assemblies, predicate, action, x => x);

        #endregion

        #region BindAllToImplementation

        public static IBinding<T> BindAllImplementationsOf<T>(this IBinder This,
                                                              Assembly assembly = null,
                                                              Predicate<Type> predicate = null,
                                                              Action<Type, IBinding> action = null) =>
            This.BindAllImplementationsOf<T>(
                assembly != null
                    ? new[] { assembly }
                    : null,
                predicate,
                action);

        public static IBinding<T> BindAllImplementationsOf<T>(this IBinder This,
                                                              Assembly[] assemblies = null,
                                                              Predicate<Type> predicate = null,
                                                              Action<Type, IBinding> action = null) =>
            This.BindAllInternal<T>(assemblies, predicate, action, _ => typeof(T));

        private static IBinding<T> BindAllInternal<T>(this IBinder This,
                                                      Assembly[] assemblies,
                                                      Predicate<Type> predicate,
                                                      Action<Type, IBinding> action,
                                                      Func<Type, Type> abstractionTypeSelector)
        {
            var types = assemblies != null
                            ? assemblies.SelectMany(x => x.GetTypes())
                                        .ToArray()
                            : typeof(T).Assembly.GetTypes();

            return new CompositeBinding(
                types.Where(x => !x.IsAbstract && x.IsAssignableTo<T>() && (predicate == null || predicate(x)))
                     .Select(
                          x =>
                          {
                              var binding = This.Bind(abstractionTypeSelector(x), x);
                              action?.Invoke(x, binding);
                              return binding;
                          })).Typed<T>();
        }

        #endregion

        #region BindReference

        public static IBinding<TSourceAbstraction> BindReference<TSourceAbstraction>(this IBinder This, BindingId id) =>
            This.BindReference(typeof(TSourceAbstraction), id)
                .Typed<TSourceAbstraction>();

        #endregion

        #region BindAsList

        public static IBinding BindList<TAbstraction>(this IBinder This, Action<IListBinder<TAbstraction>> action)
        {
            var listBinder = new ListBinder<TAbstraction>(This);
            action(listBinder);
            return listBinder.CompositeBinding;
        }

        public static IBinding BindAllAsList<TAbstraction>(this IBinder This, Assembly assembly = null)
        {
            var listBinder = new ListBinder<TAbstraction>(This);
            listBinder.BindAll(assembly);
            return listBinder.CompositeBinding;
        }

        public static IBinding BindAllAsList<TAbstraction>(this IBinder This, params Assembly[] assemblies)
        {
            var listBinder = new ListBinder<TAbstraction>(This);
            listBinder.BindAll(assemblies);
            return listBinder.CompositeBinding;
        }

        public static IBinding BindAllAsList<TAbstraction>(this IBinder This,
                                                           Func<Type, bool> exclude = null,
                                                           params Assembly[] assemblies)
        {
            var listBinder = new ListBinder<TAbstraction>(This);
            listBinder.BindAll(assemblies, exclude);
            return listBinder.CompositeBinding;
        }

        #endregion
    }
}