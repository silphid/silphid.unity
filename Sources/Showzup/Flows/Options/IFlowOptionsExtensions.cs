using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Silphid.Options;

namespace Silphid.Showzup.Flows
{
    public static class IFlowOptionsExtensions
    {
        #region WithValue/Flag

        public static IFlowOptions WithValue(this IFlowOptions This, object key, object value) =>
            value != null
                ? new FlowOptions(This, key, value)
                : This;

        public static IFlowOptions WithValue<T>(this IFlowOptions This, T value) =>
            value != null
                ? new FlowOptions(This, typeof(T), value)
                : This;

        public static IFlowOptions WithFlag(this IFlowOptions This, object key, bool value = true) =>
            This.WithValue(key, value);

        #endregion

        #region Parameters
        
        private const string ParametersKey = "Parameters";

        public static IFlowOptions WithParameter<T>(this IFlowOptions This, T value) =>
            This.WithParameter(typeof(T), value);

        public static IFlowOptions WithParameter(this IFlowOptions This, Type type, object value) =>
            value != null
                ? This.WithValue(ParametersKey, new DictionaryEntry(type, value))
                : This;

        public static IFlowOptions WithParameters(this IFlowOptions This, params object[] values) =>
            values != null
                ? This.WithValue(ParametersKey, values.ToDictionary(x => x.GetType()))
                : This;

        public static IFlowOptions WithParameters(this IFlowOptions This, IEnumerable<object> values) =>
            values != null
                ? This.WithParameters(values.ToArray())
                : This;

        public static IDictionary<Type, object> GetParameters(this IFlowOptions This) =>
            This.GetValuesAsDictionary<Type, object>(ParametersKey);

        #endregion
    }
}