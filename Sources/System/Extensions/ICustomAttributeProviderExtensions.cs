using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Silphid.Extensions
{
    public static class ICustomAttributeProviderExtensions
    {
        public static bool HasAttribute<T>(this ICustomAttributeProvider This, bool inherit = true) =>
            This.GetCustomAttributes(typeof(T), inherit)
                .Any();

        public static T GetRequiredAttribute<T>(this ICustomAttributeProvider This, bool inherit = true)
        {
            var attribute = This.GetAttribute<T>(inherit);
            if (attribute == null)
                throw new InvalidOperationException($"Missing required attribute of type {typeof(T).Name} on {This}");

            return attribute;
        }

        public static T GetAttribute<T>(this ICustomAttributeProvider This, bool inherit = true) =>
            This.GetCustomAttributes(typeof(T), inherit)
                .Cast<T>()
                .FirstOrDefault();

        public static IEnumerable<T> GetRequiredAttributes<T>(this ICustomAttributeProvider This, bool inherit = true)
        {
            var attributes = This.GetAttributes<T>(inherit)
                                 .ToList();
            if (!attributes.Any())
                throw new InvalidOperationException($"Missing required attribute of type {typeof(T).Name} on {This}");

            return attributes;
        }

        public static IEnumerable<T> GetAttributes<T>(this ICustomAttributeProvider This, bool inherit = true) =>
            This.GetCustomAttributes(typeof(T), inherit)
                .Cast<T>();
    }
}