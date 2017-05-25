//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace YamlDotNet
{
    internal static class StandardRegexOptions
    {
        public const RegexOptions Compiled = RegexOptions.None;
    }

    internal static class ReflectionExtensions
    {

#if UNITY_WSA && !UNITY_EDITOR
        public static Type BaseType(this Type This) =>
            This.GetTypeInfo().BaseType;

        public static bool IsGenericType(this Type This) =>
            This.GetTypeInfo().IsGenericType;

        public static bool IsValueType(this Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        public static bool IsInterface(this Type type)
        {
            return type.GetTypeInfo().IsInterface;
        }

        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        public static TypeCode GetTypeCode(this Type type)
        {
            if (type == null)
                return TypeCode.Empty;

            if (type.IsInstanceOf(typeof(bool))) return TypeCode.Boolean;
            if (type.IsInstanceOf(typeof(byte))) return TypeCode.Byte;
            if (type.IsInstanceOf(typeof(char))) return TypeCode.Char;
            if (type.IsInstanceOf(typeof(DateTime))) return TypeCode.DateTime;
            if (type.IsInstanceOf(typeof(decimal))) return TypeCode.Decimal;
            if (type.IsInstanceOf(typeof(double))) return TypeCode.Double;
            if (type.IsInstanceOf(typeof(short))) return TypeCode.Int16;
            if (type.IsInstanceOf(typeof(int))) return TypeCode.Int32;
            if (type.IsInstanceOf(typeof(long))) return TypeCode.Int64;
            if (type.IsInstanceOf(typeof(sbyte))) return TypeCode.SByte;
            if (type.IsInstanceOf(typeof(float))) return TypeCode.Single;
            if (type.IsInstanceOf(typeof(string))) return TypeCode.String;
            if (type.IsInstanceOf(typeof(ushort))) return TypeCode.UInt16;
            if (type.IsInstanceOf(typeof(uint))) return TypeCode.UInt32;
            if (type.IsInstanceOf(typeof(ulong))) return TypeCode.UInt64;

            return type.IsByRef ? TypeCode.Object : TypeCode.Empty;
        }

        public static MethodInfo GetPublicStaticMethod(this Type type, string name, params Type[] parameterTypes)
        {
            var method = type.GetMethod(name, parameterTypes);
            return (method.IsStatic && method.IsPublic) ? method : null;
        }

        /// <summary>
        /// Determines whether the specified type has a default constructor.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///     <c>true</c> if the type has a default constructor; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasDefaultConstructor(this Type type)
        {
            return type.IsValueType() || (type.GetConstructor(Type.EmptyTypes)?.IsPublic ?? false);
        }

        public static bool IsAssignableFrom(this Type This, Type type)
        {
            return TypeExtensions.IsAssignableFrom(This, type);
        }

        public static Type[] GetGenericArguments(this Type This)
        {
            return This.GetTypeInfo().GetGenericTypeDefinition().GenericTypeArguments;
        }

        public static Type[] GetInterfaces(this Type This)
        {
            return This.GetTypeInfo().ImplementedInterfaces.ToArray();
        }
#else
        public static Type BaseType(this Type This) =>
            This.BaseType;

        public static bool IsGenericType(this Type This) =>
            This.IsGenericType;

        public static bool IsValueType(this Type type)
        {
            return type.IsValueType;
        }

        public static bool IsInterface(this Type type)
        {
            return type.IsInterface;
        }

        public static bool IsEnum(this Type type)
        {
            return type.IsEnum;
        }

        public static TypeCode GetTypeCode(this Type type)
        {
            return Type.GetTypeCode(type);
        }

        public static MethodInfo GetPublicStaticMethod(this Type type, string name, params Type[] parameterTypes)
        {
            return type.GetMethod(name, BindingFlags.Public | BindingFlags.Static, null, parameterTypes, null);
        }

        /// <summary>
        /// Determines whether the specified type has a default constructor.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///     <c>true</c> if the type has a default constructor; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasDefaultConstructor(this Type type)
        {
            return type.IsValueType || type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null) != null;
        }
#endif

        public static PropertyInfo GetPublicProperty(this Type type, string name)
        {
            return type.GetProperty(name);
        }

        public static IEnumerable<PropertyInfo> GetPublicProperties(this Type type)
        {
            var instancePublic = BindingFlags.Instance | BindingFlags.Public;
            return type.IsInterface()
                ? (new[] { type })
                    .Concat(type.GetInterfaces())
                    .SelectMany(i => i.GetProperties(instancePublic))
                : type.GetProperties(instancePublic);
        }

        public static IEnumerable<MethodInfo> GetPublicStaticMethods(this Type type)
        {
            return type.GetMethods(BindingFlags.Static | BindingFlags.Public);
        }

        public static MethodInfo GetPublicInstanceMethod(this Type type, string name)
        {
            return type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance);
        }

        private static readonly FieldInfo remoteStackTraceField = typeof(Exception)
                .GetField("_remoteStackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);

        public static Exception Unwrap(this TargetInvocationException ex)
        {
            remoteStackTraceField?.SetValue(ex.InnerException, ex.InnerException.StackTrace + "\r\n");
            return ex.InnerException;
        }

        public static bool IsInstanceOf(this Type type, object o)
        {
            return type.IsInstanceOfType(o);
        }
    }

    internal sealed class CultureInfoAdapter : CultureInfo
    {
        private readonly IFormatProvider _provider;

        public CultureInfoAdapter(CultureInfo baseCulture, IFormatProvider provider)
            : base(baseCulture.Name)
        {
            _provider = provider;
        }

        public override object GetFormat(Type formatType)
        {
            return _provider.GetFormat(formatType);
        }
    }

    internal static class PropertyInfoExtensions
    {
        public static object ReadValue(this PropertyInfo property, object target)
        {
            return property.GetGetMethod().Invoke(target, null);
        }
    }
}
