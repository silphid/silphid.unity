using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Injexit
{
    public class Reflector : IReflector
    {
        private const BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private readonly InjectTypeInfo _gameObjectTypeInfo;
        private readonly Dictionary<Type, InjectTypeInfo> _typeInfos = new Dictionary<Type, InjectTypeInfo>();

        public Reflector()
        {
            _gameObjectTypeInfo = new InjectTypeInfo(
                typeof(GameObject),
                new InjectConstructorInfo(null,
                    new InvalidOperationException("GameObject cannot be instantiated manually."), null),
                Array.Empty<InjectMethodInfo>(),
                Array.Empty<InjectFieldOrPropertyInfo>());
        }

        public InjectTypeInfo GetTypeInfo(Type type)
        {
            if (type == typeof(GameObject))
                return _gameObjectTypeInfo;
            
            var typeInfo = _typeInfos.GetValueOrDefault(type);
            
            if (typeInfo == null)
            {
                typeInfo = new InjectTypeInfo(
                    type,
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
            
            constructors = constructors.Where(x => x.HasAttribute<InjectAttribute>()).ToArray();
            if (constructors.Length == 1)
                return constructors.First();

            if (constructors.Length > 1)
                throw new InvalidOperationException(
                    $"Type {type.Name} has multiple constructors marked with an [Inject] attribute.");
            
            throw new InvalidOperationException(
                $"Type {type.Name} has multiple constructors, but none of them is marked with an [Inject] attribute to make it the default.");
        }

        private IEnumerable<InjectMethodInfo> GetMethods(Type type)
        {
            var methods = type.GetMethods(_bindingFlags);
            foreach (var method in methods)
            {
                var attribute = method.GetAttribute<InjectAttribute>();
                if (attribute != null)
                    yield return new InjectMethodInfo(method, GetParameters(method));
            }
        }

        private InjectParameterInfo[] GetParameters(MethodBase method) =>
            (from parameter in method.GetParameters()
                let isOptional = parameter.HasAttribute<OptionalAttribute>() || parameter.IsOptional
                select new InjectParameterInfo(parameter, isOptional))
            .ToArray();

        private InjectFieldOrPropertyInfo[] GetFieldsAndProperties(Type type) =>
            GetFields(type).Concat(GetProperties(type)).ToArray();

        private IEnumerable<InjectFieldOrPropertyInfo> GetFields(Type type)
        {
            var fields = type.GetFields(_bindingFlags);
            foreach (var field in fields)
            {
                var attribute = field.GetAttribute<InjectAttribute>();
                if (attribute != null)
                {
                    var isOptional = field.HasAttribute<OptionalAttribute>();
                    yield return new InjectFieldInfo(field, field.FieldType, isOptional);
                }
            }
        }

        private IEnumerable<InjectFieldOrPropertyInfo> GetProperties(Type type)
        {
            var properties = type.GetProperties(_bindingFlags);
            foreach (var property in properties)
            {
                var attribute = property.GetAttribute<InjectAttribute>();
                if (attribute != null)
                {
                    if (!property.CanWrite)
                        throw new InvalidOperationException($"Property {type.Name}.{property.Name} is marked with [Inject] attribute, but has not setter.");

                    var isOptional = property.HasAttribute<OptionalAttribute>();
                    yield return new InjectPropertyInfo(property, property.PropertyType, isOptional);
                }
            }
        }

        public static string GetCanonicalName(string name) =>
            name.RemovePrefix("_").ToUpperFirst();
    }
}