using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Silphid.Extensions
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetConcreteTypesOf<T>(this Assembly This) =>
            This.GetTypes().Where(x =>
                !x.IsAbstract &&
                 !x.IsGenericTypeDefinition &&
                 x.IsAssignableTo<T>());
    }
}