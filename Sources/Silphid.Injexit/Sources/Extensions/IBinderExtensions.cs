using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silphid.Extensions;

namespace Silphid.Injexit
{
    public static class IBinderExtensions
    {
        #region BindInstance(s)

        public static IBinding BindInstance<T>(this IBinder This, T instance) =>
            This.BindInstance(typeof(T), instance);

        public static IBinding BindInstance(this IBinder This, object instance) =>
            This.BindInstance(instance.GetType(), instance);

        public static IBinding BindInstances(this IBinder This, IEnumerable<object> instances) =>
            instances != null
                ? new CompositeBinding(instances.Select(This.BindInstance))
                : Binding.Null;

        public static IBinding BindInstances(this IBinder This, IDictionary<Type, object> instances) =>
            instances != null
                ? new CompositeBinding(instances.Select(x => This.BindInstance(x.Key, x.Value)))
                : Binding.Null;

        #endregion

        #region BindOptionalInstance(s)

        public static IBinding BindOptionalInstance<T>(this IBinder This, T instance) =>
            instance != null ? This.BindInstance(typeof(T), instance) : Binding.Null;

        public static IBinding BindOptionalInstance(this IBinder This, object instance) =>
            instance != null ? This.BindInstance(instance.GetType(), instance) : Binding.Null;

        public static IBinding BindOptionalInstances(this IBinder This, IEnumerable<object> instances) =>
            instances != null
                ? new CompositeBinding(instances.WhereNotNull().Select(This.BindInstance))
                : Binding.Null;

        #endregion

        #region Bind

        public static IBinding Bind<TAbstraction>(this IBinder This, Type concretionType) =>
            This.Bind(typeof(TAbstraction), concretionType);

        public static IBinding Bind<TAbstraction, TConcretion>(this IBinder This) where TConcretion : TAbstraction =>
            This.Bind(typeof(TAbstraction), typeof(TConcretion));

        #endregion
        
        #region BindToSelf

        public static IBinding BindToSelf<T>(this IBinder This) =>
            This.Bind<T, T>();

        public static IBinding BindAnonymous<T>(this IBinder This) =>
            This.BindToSelf<T>();

        public static void BindToSelfAll<T>(this IBinder This, Assembly assembly = null, Predicate<Type> predicate = null)
        {
            var types = (assembly ?? typeof(T).Assembly).GetTypes();
            types
                .Where(x => !x.IsAbstract && x.IsAssignableTo<T>() && (predicate == null || predicate(x)))
                .ForEach(x => This.Bind(x, x));
        }

        #endregion

        #region BindReference

        public static IBinding BindReference<TSourceAbstraction>(this IBinder This, BindingId id) =>
            This.BindReference(typeof(TSourceAbstraction), id);

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

        #endregion
        
        #region Typed factories

        public static IBinding BindTypedFactory<TAbstraction>(this IBinder This) =>
            This.BindInstance<Func<Type, TAbstraction>>(type => (TAbstraction) This.Resolve(type));

        public static IBinding BindTypedFactory<T1, TAbstraction>(this IBinder This) =>
            This.BindInstance<Func<Type, T1, TAbstraction>>((type, t1) => (TAbstraction) This
                .UsingInstance(t1).Resolve(type));

        public static IBinding BindTypedFactory<T1, T2, TAbstraction>(this IBinder This) =>
            This.BindInstance<Func<Type, T1, T2, TAbstraction>>((type, t1, t2) => (TAbstraction) This
                .UsingInstances(t1, t2).Resolve(type));

        public static IBinding BindTypedFactory<T1, T2, T3, TAbstraction>(this IBinder This) =>
            This.BindInstance<Func<Type, T1, T2, T3, TAbstraction>>((type, t1, t2, t3) => (TAbstraction) This
                .UsingInstances(t1, t2, t3).Resolve(type));

        public static IBinding BindTypedFactoryWithParameters<TAbstraction>(this IBinder This) =>
            This.BindInstance<Func<Type, object[], TAbstraction>>((type, instances) => (TAbstraction) This
                .UsingInstances(instances).Resolve(type));

        public static IBinding BindTypedFactoryWithParameters<TFactoryAbstraction, TFactory, TAbstraction>(this IBinder This)
            where TFactory : TFactoryAbstraction =>
            This.Bind<TFactoryAbstraction, TFactory>()
                .Using(x => x.BindInstance<Func<Type, object[], TAbstraction>>((type, instances) => (TAbstraction) This
                    .UsingInstances(instances).Resolve(type)));

        #endregion

        #region Default factories

        public static IBinding BindDefaultFactory<TAbstraction>(this IBinder This) =>
            This.BindInstance<Func<TAbstraction>>(() => This.Resolve<TAbstraction>());

        public static IBinding BindDefaultFactory<T1, TAbstraction>(this IBinder This) =>
            This.BindInstance<Func<T1, TAbstraction>>(t1 => This
                .UsingInstance(t1)
                .Resolve<TAbstraction>());
                
        public static IBinding BindDefaultFactory<T1, T2, TAbstraction>(this IBinder This) =>
            This.BindInstance<Func<T1, T2, TAbstraction>>((t1, t2) => This
                .UsingInstances(t1, t2)
                .Resolve<TAbstraction>());
                
        public static IBinding BindDefaultFactory<T1, T2, T3, TAbstraction>(this IBinder This) =>
            This.BindInstance<Func<T1, T2, T3, TAbstraction>>((t1, t2, t3) => This
                .UsingInstances(t1, t2, t3)
                .Resolve<TAbstraction>());

        #endregion
    }
}