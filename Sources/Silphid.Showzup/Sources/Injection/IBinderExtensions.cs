using System;
using System.Collections.Generic;
using Silphid.Extensions;

namespace Silphid.Showzup.Injection
{
    public static class IBinderExtensions
    {
        #region BindInstance(s)

        public static void BindInstance<T>(this IBinder This, T instance) =>
            This.BindInstance(typeof(T), instance);

        public static void BindInstance(this IBinder This, object instance) =>
            This.BindInstance(instance.GetType(), instance);

        public static void BindInstances(this IBinder This, IEnumerable<object> instances) =>
            instances.ForEach(This.BindInstance);

        public static void BindInstances(this IBinder This, params object[] instances) =>
            This.BindInstances((IEnumerable<object>) instances);

        #endregion

        #region BindInstance(s)AsList

        public static void BindInstanceAsList<T>(this IBinder This, T instance) =>
            This.BindInstanceAsList(typeof(T), instance);

        public static void BindInstanceAsList(this IBinder This, object instance) =>
            This.BindInstanceAsList(instance.GetType(), instance);

        public static void BindInstancesAsList(this IBinder This, IEnumerable<object> instances) =>
            instances.ForEach(This.BindInstanceAsList);

        public static void BindInstancesAsList(this IBinder This, params object[] instances) =>
            This.BindInstancesAsList((IEnumerable<object>) instances);

        #endregion

        #region Bind

        public static void Bind<TAbstraction>(this IBinder This, Type concretionType, IResolver overrideResolver = null) =>
            This.Bind(typeof(TAbstraction), concretionType, overrideResolver);

        public static void Bind<TAbstraction, TConcretion>(this IBinder This, IResolver overrideResolver = null) where TConcretion : TAbstraction =>
            This.Bind(typeof(TAbstraction), typeof(TConcretion), overrideResolver);

        #endregion

        #region BindAsList

        public static void BindAsList<TAbstraction>(this IBinder This, Type concretionType, IResolver overrideResolver = null) =>
            This.BindAsList(typeof(TAbstraction), concretionType, overrideResolver);

        public static void BindAsList<TAbstraction, TConcretion>(this IBinder This, IResolver overrideResolver = null) where TConcretion : TAbstraction =>
            This.BindAsList(typeof(TAbstraction), typeof(TConcretion), overrideResolver);

        #endregion
        
        #region BindSelf

        public static void BindSelf<T>(this IBinder This, IResolver overrideResolver = null) =>
            This.Bind<T, T>(overrideResolver);

        #endregion

        #region BindSingle

        public static void BindSingle<TAbstraction>(this IBinder This, Type concretionType, IResolver overrideResolver = null) =>
            This.BindSingle(typeof(TAbstraction), concretionType, overrideResolver);

        public static void BindSingle<TAbstraction, TConcretion>(this IBinder This, IResolver overrideResolver = null) where TConcretion : TAbstraction =>
            This.BindSingle(typeof(TAbstraction), typeof(TConcretion), overrideResolver);

        #endregion

        #region BindSingleAsList

        public static void BindSingleAsList<TAbstraction>(this IBinder This, Type concretionType, IResolver overrideResolver = null) =>
            This.BindSingleAsList(typeof(TAbstraction), concretionType, overrideResolver);

        public static void BindSingleAsList<TAbstraction, TConcretion>(this IBinder This, IResolver overrideResolver = null) where TConcretion : TAbstraction =>
            This.BindSingleAsList(typeof(TAbstraction), typeof(TConcretion), overrideResolver);

        #endregion
    }
}