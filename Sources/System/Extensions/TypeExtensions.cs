using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if UNITY_WSA && !UNITY_EDITOR
using System.Reflection;
#endif

namespace Silphid.Extensions
{
    public static class TypeExtensions
    {
        public static T CreateInstance<T>(this Type This) =>
            (T) Activator.CreateInstance(This);

        public static bool IsAssignableFrom<T>(this Type This)
        {
            return This.IsAssignableFrom(typeof(T));
        }

        public static bool IsAssignableTo(this Type This, Type toType)
        {
            return toType.IsAssignableFrom(This);
        }

        public static bool IsAssignableTo<T>(this Type This)
        {
            return typeof(T).IsAssignableFrom(This);
        }

        public static IEnumerable<Type> WhereAssignableFrom(this IEnumerable<Type> This, Type type) =>
            This.Where(x => x.IsAssignableFrom(type));

        public static IEnumerable<Type> WhereAssignableFrom<T>(this IEnumerable<Type> This) =>
            This.WhereAssignableFrom(typeof(T));

        public static IEnumerable<Type> WhereAssignableTo(this IEnumerable<Type> This, Type type) =>
            This.Where(x => x.IsAssignableTo(type));

        public static IEnumerable<Type> WhereAssignableTo<T>(this IEnumerable<Type> This) =>
            This.WhereAssignableTo(typeof(T));

        public static bool Is<T>(this Type This)
        {
            return This == typeof(T);
        }

        public static IEnumerable<Type> Ancestors(this Type This)
        {
            This = This?.GetBaseType();
            while (This != null)
            {
                yield return This;
                This = This.GetBaseType();
            }
        }

        public static IEnumerable<Type> Ancestors<T>(this Type This) =>
            This.Ancestors()
                .Where(x => x.IsAssignableTo<T>());

        public static IEnumerable<Type> SelfAndAncestors(this Type This)
        {
            while (This != null)
            {
                yield return This;
                This = This.GetBaseType();
            }
        }

        public static IEnumerable<Type> SelfAndAncestors<T>(this Type This) =>
            This.SelfAndAncestors()
                .Where(x => x.IsAssignableTo<T>());

        public static IEnumerable<FieldInfo> GetAllInstanceFields(this Type This)
        {
            while (true)
            {
                foreach (var fieldInfo in This.GetDeclaredInstanceFields())
                    yield return fieldInfo;

                var baseType = This.GetBaseType();
                if (baseType == null || baseType == typeof(object))
                    break;

                This = baseType;
            }
        }

        public static FieldInfo[] GetDeclaredInstanceFields(this Type This)
        {
#if UNITY_WSA && !UNITY_EDITOR
            return This
                .GetRuntimeFields()
                .Where(x => x.DeclaringType == This && !x.IsStatic)
                .ToArray();
#else
            return This.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
#endif
        }

#if UNITY_WSA && !UNITY_EDITOR
        public static Type GetBaseType(this Type This) =>
            This.GetTypeInfo().BaseType;

        public static IEnumerable<T> GetAttributes<T>(this Type This, bool inherit = true) =>
            This.GetTypeInfo().GetCustomAttributes(typeof(T), inherit).Cast<T>();

        public static bool IsGenericType(this Type This) =>
            This.GetTypeInfo().IsGenericType;

        public static bool IsAbstract(this Type This) =>
            This.GetTypeInfo().IsAbstract;
#else
        public static Type GetBaseType(this Type This) =>
            This.BaseType;

        public static bool IsGenericType(this Type This) =>
            This.IsGenericType;

        public static bool IsAbstract(this Type This) =>
            This.IsAbstract;
#endif
    }
}