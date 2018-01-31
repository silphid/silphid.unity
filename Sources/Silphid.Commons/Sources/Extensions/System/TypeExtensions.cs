using System;
using System.Collections.Generic;
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

        public static bool Is<T>(this Type This)
        {
            return This == typeof(T);
        }

        public static IEnumerable<Type> Ancestors(this Type type)
        {
            type = type?.GetBaseType();
            while (type != null)
            {
                yield return type;
                type = type.GetBaseType();
            }
        }

        public static IEnumerable<Type> SelfAndAncestors(this Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.GetBaseType();
            }
        }

        public static IEnumerable<FieldInfo> GetAllInstanceFields(this Type type)
        {
            while (true)
            {
                foreach (var fieldInfo in type.GetDeclaredInstanceFields())
                    yield return fieldInfo;

                var baseType = type.GetBaseType();
                if (baseType == null || baseType == typeof(object))
                    break;
                
                type = baseType;
            }
        }

        public static FieldInfo[] GetDeclaredInstanceFields(this Type type)
        {
#if UNITY_WSA && !UNITY_EDITOR
            return type
                .GetRuntimeFields()
                .Where(x => x.DeclaringType == type && !x.IsStatic)
                .ToArray();
#else
            return type.GetFields(
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