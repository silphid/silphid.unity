using System;
using System.Collections.Generic;
using System.Linq;

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

        public static IBinding BindInstances(this IBinder This, params object[] instances) =>
            This.BindInstances((IEnumerable<object>) instances);

        #endregion

        #region Bind

        public static IBinding Bind<TAbstraction>(this IBinder This, Type concretionType) =>
            This.Bind(typeof(TAbstraction), concretionType);

        public static IBinding Bind<TAbstraction, TConcretion>(this IBinder This) where TConcretion : TAbstraction =>
            This.Bind(typeof(TAbstraction), typeof(TConcretion));

        #endregion
        
        #region BindSelf

        public static IBinding BindSelf<T>(this IBinder This) =>
            This.Bind<T, T>();

        #endregion
    }
}