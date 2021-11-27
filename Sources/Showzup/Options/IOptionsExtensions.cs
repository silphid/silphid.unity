using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using Silphid.Options;
using UnityEngine;

namespace Silphid.Showzup
{
    public static class IOptionsExtensions
    {
        #region WithValue/Flag

        public static IOptions WithValue(this IOptions This, object key, object value) =>
            value != null
                ? new Options(This, key, value)
                : This;

        public static IOptions WithValue<T>(this IOptions This, T value) =>
            value != null
                ? new Options(This, typeof(T), value)
                : This;

        public static IOptions WithFlag(this IOptions This, object key, bool value = true) =>
            This.WithValue(key, value);

        #endregion

        #region IPresenter extensions

        public static IPresenter WithOptions(this IPresenter This, Func<IOptions, IOptions> selector) =>
            new OptionsDecoratorPresenter(This, selector);

        public static IPresenter WithOptions(this IPresenter This, Func<object, IOptions, IOptions> selector) =>
            new OptionsDecoratorPresenter(This, selector);

        public static IPresenter WithFlag(this IPresenter This, object key, bool value = true) =>
            This.WithOptions(x => x.WithValue(key, value));

        #endregion

        #region Direction

        public static IPresenter With(this IPresenter This, Direction? value) =>
            This.WithOptions(x => x.With(value));

        public static IOptions With(this IOptions This, Direction? value) =>
            This.WithValue(value);

        public static Direction? GetDirection(this IOptions This) =>
            This.GetValue<Direction?>();
        
        public static GameObject GetSource(this IOptions This) =>
            This.GetValue<GameObject>();
        
        public static IOptions With(this IOptions This, GameObject value) =>
            This.WithValue(value);

        public static Direction GetDirectionOrDefault(this IOptions This) =>
            This.GetDirection() ?? Direction.Default;

        #endregion

        #region History

        private const string HistoryInputKey = "HistoryInput";

        public static IPresenter WithHistoryInput(this IPresenter This, object input) =>
            This.WithOptions(x => x.WithHistoryInput(input));

        public static IOptions WithHistoryInput(this IOptions This, object input) =>
            This.WithValue(HistoryInputKey, input);

        public static object GetHistoryInput(this IOptions This) =>
            This.GetValue<object>(HistoryInputKey, null);

        #endregion

        #region PushMode

        public static IPresenter With(this IPresenter This, PushMode? value) =>
            This.WithOptions(x => x.With(value));

        public static IOptions With(this IOptions This, PushMode? value) =>
            This.WithValue(value);

        public static PushMode? GetPushMode(this IOptions This) =>
            This.GetValue<PushMode?>();

        #endregion

        #region Variants

        public static IPresenter With(this IPresenter This, VariantSet value) =>
            This.WithOptions(x => x.With(value));

        public static IPresenter With(this IPresenter This, IEnumerable<IVariant> values) =>
            This.WithOptions(x => x.With(values));

        public static IPresenter With(this IPresenter This, params IVariant[] values) =>
            This.WithOptions(x => x.With(values));

        public static IOptions With(this IOptions This, VariantSet value) =>
            value.Count > 0
                ? This.WithValue(value)
                : This;

        public static IOptions With(this IOptions This, IEnumerable<IVariant> values)
        {
            var variants = values?.WhereNotNull()
                                  .ToList();
            return variants?.Count > 0
                       ? This.WithValue(new VariantSet(variants))
                       : This;
        }

        public static IOptions With(this IOptions This, params IVariant[] values) =>
            This.With((IEnumerable<IVariant>) values);

        public static VariantSet GetVariants(this IOptions This) =>
            new VariantSet(This.GetValues<IVariant>(typeof(VariantSet)));

        #endregion

        #region Transition

        public static IPresenter With(this IPresenter This, ITransition value) =>
            This.WithOptions(x => x.With(value));

        public static IPresenter With(this IPresenter This, Func<object, IOptions, ITransition> selector) =>
            This.WithOptions((input, options) => options.With(selector(input, options)));

        public static IPresenter WithTransitionForInputOfType<T>(this IPresenter This, ITransition transition) =>
            This.WithOptions(
                (input, options) => input is T
                                        ? options.With(transition)
                                        : options);

        public static IOptions With(this IOptions This, ITransition value) =>
            This.WithValue(value);

        public static ITransition GetTransition(this IOptions This) =>
            This.GetValue<ITransition>();

        #endregion

        #region TransitionDuration

        private const string TransitionDurationKey = "TransitionDuration";

        public static IPresenter WithTransitionDuration(this IPresenter This, float? value) =>
            This.WithOptions(x => x.WithTransitionDuration(value));

        public static IOptions WithTransitionDuration(this IOptions This, float? value) =>
            This.WithValue(TransitionDurationKey, value);

        public static float? GetTransitionDuration(this IOptions This) =>
            This.GetValue<float?>(TransitionDurationKey, null);

        #endregion

        #region Parameters

        private const string ParametersKey = "Parameters";

        public static IPresenter WithParameter(this IPresenter This, Type key, object value) =>
            This.WithOptions(x => x.WithParameter(key, value));

        public static IPresenter WithParameter<T>(this IPresenter This, T value) =>
            This.WithOptions(x => x.WithParameter(value));

        public static IPresenter WithOptionalParameter<T>(this IPresenter This, T value) =>
            value != null
                ? This.WithOptions(x => x.WithParameter(value))
                : This;

        public static IOptions WithParameter(this IOptions This, Type key, object value) =>
            This.WithValue(ParametersKey, new DictionaryEntry(key, value));

        public static IOptions WithParameter<T>(this IOptions This, T value) =>
            This.WithParameter(typeof(T), value);

        public static IOptions WithParameters(this IOptions This, Dictionary<Type, object> values) =>
            This.WithValue(ParametersKey, values);

        public static IOptions WithParameters(this IOptions This, params object[] values) =>
            This.WithValue(ParametersKey, values.ToDictionary(x => x.GetType(), x => x));

        public static IOptions WithOptionalParameters(this IOptions This, params object[] values) =>
            This.WithParameters(values.WhereNotNull());

        public static IDictionary<Type, object> GetParameters(this IOptions This) =>
            This.GetValuesAsDictionary<Type, object>(ParametersKey);

        #endregion

        #region ViewOptions

        public static IPresenter With(this IPresenter This, IViewOptions value) =>
            This.WithOptions(x => x.With(value));

        public static IOptions With(this IOptions This, IViewOptions value) =>
            This.WithValue(value);

        public static IOptions WithViewOptions(this IOptions This, IDictionary<object, object> value) =>
            value != null && value.Count != 0
                ? This.With(new DictionaryViewOptions(value))
                : This;

        public static IViewOptions GetViewOptions(this IOptions This) =>
            This.GetValue<IViewOptions>();

        #endregion
    }
}