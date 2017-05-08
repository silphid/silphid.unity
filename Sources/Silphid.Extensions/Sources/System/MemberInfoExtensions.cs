using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Silphid.Extensions
{
    public static class MemberInfoExtensions
    {
        public static bool HasAttribute<T>(this MemberInfo member, bool inherit = true) =>
            member.GetCustomAttributes(typeof(T), inherit).Any();

        public static T GetRequiredAttribute<T>(this MemberInfo member, bool inherit = true)
        {
            var attribute = member.GetAttribute<T>(inherit);
            if (attribute == null)
                throw new InvalidOperationException($"Missing required attribute of type {typeof(T).Name} on member {member.Name}");

            return attribute;
        }

        public static T GetAttribute<T>(this MemberInfo member, bool inherit = true) =>
            member.GetCustomAttributes(typeof(T), inherit).Cast<T>().FirstOrDefault();

        public static IEnumerable<T> GetRequiredAttributes<T>(this MemberInfo member, bool inherit = true)
        {
            var attributes = member.GetAttributes<T>(inherit).ToList();
            if (!attributes.Any())
                throw new InvalidOperationException(
                    $"Missing required attribute of type {typeof(T).Name} on member {member.Name}");

            return attributes;
        }

        public static IEnumerable<T> GetAttributes<T>(this MemberInfo member, bool inherit = true) =>
            member.GetCustomAttributes(typeof(T), inherit).Cast<T>();
    }
}