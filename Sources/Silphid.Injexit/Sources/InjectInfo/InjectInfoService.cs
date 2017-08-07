using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silphid.Extensions;

namespace Silphid.Injexit
{
    public class InjectInfoService : IInjectInfoService
    {
        private readonly Dictionary<Type, InjectTypeInfo> _typeInfos = new Dictionary<Type, InjectTypeInfo>();
        
        public InjectTypeInfo GetTypeInfo(Type type)
        {
            var typeInfo = _typeInfos.GetOptionalValue(type);
            
            if (typeInfo == null)
            {
                typeInfo = new InjectTypeInfo(
                    GetConstructor(type),
                    GetMethods(type).ToArray(),
                    GetFieldsAndProperties(type));

                _typeInfos[type] = typeInfo;
            }

            return typeInfo;
        }

        private InjectConstructorInfo GetConstructor(Type type)
        {
            try
            {
                var constructor = ResolveConstructor(type);
                return new InjectConstructorInfo(constructor, null, GetParameters(constructor));
            }
            catch (InvalidOperationException exception)
            {
                return new InjectConstructorInfo(null, exception, null);
            }
        }

        private ConstructorInfo ResolveConstructor(Type type)
        {
            var constructors = type.GetConstructors();
            
            if (constructors.Length == 0)
                throw new InvalidOperationException(
                    $"Type {type.Name} has no constructors, probably because it was stripped at build-time (consider adding the type to your bind.xml file).");
            
            if (constructors.Length == 1)
                return constructors.First();
            
            var constructor = constructors.FirstOrDefault(x => x.HasAttribute<InjectAttribute>());
            if (constructor == null)
                throw new InvalidOperationException(
                    $"Type {type.Name} has multiple constructors, but none of them is marked with an injection attribute to make it the default.");

            return constructor;
        }

        private IEnumerable<InjectMethodInfo> GetMethods(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var attribute = method.GetAttribute<InjectAttribute>();
                if (attribute != null)
                    yield return new InjectMethodInfo(method, GetParameters(method));
            }
        }

        private InjectMemberInfo[] GetParameters(MethodBase method) =>
            (from parameter in method.GetParameters()
                let attribute = parameter.GetAttribute<InjectAttribute>()
                select new InjectMemberInfo(
                    parameter.ParameterType,
                    attribute?.IsOptional ?? false,
                    attribute?.Id))
            .ToArray();

        private InjectFieldOrPropertyInfo[] GetFieldsAndProperties(Type type) =>
            GetFields(type).Concat(GetProperties(type)).ToArray();

        private IEnumerable<InjectFieldOrPropertyInfo> GetFields(Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var attribute = field.GetAttribute<InjectAttribute>();
                if (attribute != null)
                    yield return new InjectFieldInfo(field, field.FieldType, attribute.IsOptional, attribute.Id);
            }
        }

        private IEnumerable<InjectFieldOrPropertyInfo> GetProperties(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var attribute = property.GetAttribute<InjectAttribute>();
                if (attribute != null)
                    yield return new InjectPropertyInfo(property, property.PropertyType, attribute.IsOptional, attribute.Id);
            }
        }
    }
}