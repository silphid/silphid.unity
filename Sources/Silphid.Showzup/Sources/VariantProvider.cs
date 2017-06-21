using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Showzup
{
    public class VariantProvider : IVariantProvider
    {
        public List<IVariantGroup> AllVariantGroups { get; }
        public ReactiveProperty<VariantSet> GlobalVariants { get; } =
            new ReactiveProperty<VariantSet>(VariantSet.Empty);

        public VariantProvider(params IVariantGroup[] allVariantGroups)
        {
            AllVariantGroups = allVariantGroups.ToList();
        }

        public static IVariantProvider From<T1>() => From(typeof(T1));
        public static IVariantProvider From<T1, T2>() => From(typeof(T1), typeof(T2));
        public static IVariantProvider From<T1, T2, T3>() => From(typeof(T1), typeof(T2), typeof(T3));
        public static IVariantProvider From<T1, T2, T3, T4>() => From(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        public static IVariantProvider From<T1, T2, T3, T4, T5>() => From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

        public static IVariantProvider From(params Type[] variantTypes) =>
            new VariantProvider(
                variantTypes
                    .Select(GetVariantGroupFromVariantType)
                    .ToArray());

        private static IVariantGroup GetVariantGroupFromVariantType(Type type)
        {
            var field = type.GetField("Group", BindingFlags.Public | BindingFlags.Static);
            if (field == null || !field.FieldType.IsAssignableTo<IVariantGroup>())
                throw new InvalidOperationException($"Variant type {type.Name} must have a static Group field of type IVariantGroup.");

            return (IVariantGroup) field.GetValue(null);
        }
    }
}