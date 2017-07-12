using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Injexit
{
    public class Container : IContainer
    {
        public static readonly IContainer Null = new NullContainer();
        
        #region Private fields

        private readonly List<Binding> _bindings = new List<Binding>();
        private readonly Dictionary<Type, Type> _forwards = new Dictionary<Type, Type>();
        private readonly ILogger _logger;
        private static readonly Func<IResolver, object> NullFactory = null;

        #endregion

        #region Constructors

        public Container(ILogger logger = null)
        {
            _logger = logger;
        }

        #endregion

        #region IContainer members

        public IContainer Create() =>
            new Container(_logger);

        #endregion
        
        #region IBinder members

        public void BindForward(Type sourceAbstractionType, Type targetAbstractionType)
        {
            if (_forwards.ContainsKey(sourceAbstractionType))
                throw new InvalidOperationException($"Cannot specify same source abstraction type {sourceAbstractionType.Name} in more than one forward binding.");

            _forwards[sourceAbstractionType] = targetAbstractionType;
        }

        public IBinding BindInstance(Type abstractionType, object instance)
        {
            if (!instance.GetType().IsAssignableTo(abstractionType))
                throw new InvalidOperationException($"Instance type {instance.GetType().Name} must be assignable to abstraction type {abstractionType.Name}.");

            var binding = new Binding(this, abstractionType, instance.GetType())
            {
                Instance = instance,
                Lifetime = Lifetime.Single
            };
            _bindings.Add(binding);
            
            abstractionType.GetAttributes<BindAttribute>()
                .Select(x => x.Type)
                .ForEach(x => BindInstance(x, instance));

            return binding;
        }

        public IBinding Bind(Type abstractionType, Type concretionType)
        {
            if (!concretionType.IsAssignableTo(abstractionType))
                throw new InvalidOperationException($"Concretion type {concretionType.Name} must be assignable to abstraction type {abstractionType.Name}.");

            if (concretionType.IsAbstract)
                throw new InvalidOperationException($"Concretion type {concretionType.Name} cannot be abstract.");

            var binding = new Binding(this, abstractionType, concretionType);
            _bindings.Add(binding);

            return binding;
        }

        #endregion
        
        #region IResolver members

        public Func<IResolver, object> ResolveFactory(Type abstractionType, string id = null, bool isOptional = false)
        {
            _logger?.Log($"Resolving {abstractionType.Name}...");

            abstractionType = ResolveForward(abstractionType);

            return ResolveFromTypeMappings(abstractionType, id) ??
                   ResolveFromListMappings(abstractionType) ??
                   ThrowIfNotOptional(abstractionType, isOptional);
        }

        private Type ResolveForward(Type abstractionType) =>
            _forwards.GetOptionalValue(abstractionType) ?? abstractionType;

        private Func<IResolver, object> ThrowIfNotOptional(Type abstractionType, bool isOptional)
        {
            if (!isOptional)
                throw new Exception($"No binding for required type {abstractionType.Name}.");

            return NullFactory;
        }

        private Func<IResolver, object> ResolveFromListMappings(Type abstractionType)
        {
            var elementType = GetListElementType(abstractionType);
            if (elementType == null)
                return NullFactory;

            var factories = GetListFactories(elementType);
            if (factories.Count == 0)
                return NullFactory;
            
            // Array
            if (abstractionType.IsArray)
                return resolver => ToTypedArray(factories.Select(x => x(resolver)), elementType);

            // Enumerable/List
            return resolver => ToTypedList(factories.Select(x => x(resolver)), elementType);
        }

        private object ToTypedArray(IEnumerable<object> objects, Type elementType)
        {
            var elements = objects.ToArray();
            var array = Array.CreateInstance(elementType, elements.Length);
            Array.Copy(elements, array, elements.Length);
            return array;
        }

        private object ToTypedList(IEnumerable<object> objects, Type elementType)
        {
            var listType = typeof(List<>).MakeGenericType(elementType);
            return Activator.CreateInstance(listType, ToTypedArray(objects, elementType));
        }

        private Type GetListElementType(Type abstractionType)
        {
            if (abstractionType.IsArray)
                return abstractionType.GetElementType();

            if (!abstractionType.IsGenericType)
                return null;
            
            var typeDef = abstractionType.GetGenericTypeDefinition();
            if (typeDef == typeof(IEnumerable<>) || typeDef == typeof(IList<>) || typeDef == typeof(List<>))
                return abstractionType.GetGenericArguments().First();

            return null;
        }

        private List<Func<IResolver, object>> GetListFactories(Type abstractionType) =>
            _bindings
                .Where(x => x.AbstractionType == abstractionType && x.IsList)
                .Select(ResolveFactoryInternal)
                .ToList();

        private Func<IResolver, object> ResolveFromTypeMappings(Type abstractionType, string id) =>
            ResolveFactoryInternal(ResolveType(abstractionType, id));

        private Binding ResolveType(Type abstractionType, string id)
        {
            var binding = _bindings.FirstOrDefault(x =>
                x.AbstractionType == abstractionType &&
                x.Id == id);
            
            if (binding != null)
                _logger?.Log($"Resolved {binding}");

            return binding;
        }

        private Func<IResolver, object> ResolveFactoryInternal(Binding binding)
        {
            if (binding == null)
                return NullFactory;
            
            if (binding.Lifetime == Lifetime.Transient)
                return ResolveFactoryInternal(binding.ConcretionType, binding.OverrideResolver);

            return resolver =>
                binding.Instance
                ?? (binding.Instance = ResolveFactoryInternal(binding.ConcretionType, binding.OverrideResolver).Invoke(resolver));
        }

        private Func<IResolver, object> ResolveFactoryInternal(Type concretionType, IResolver overrideResolver)
        {
            var factory = ResolveFactoryInternal(concretionType);
            return factory != null
                ? resolver => factory(resolver.Using(overrideResolver))
                : NullFactory;
        }

        private Func<IResolver, object> ResolveFactoryInternal(Type concretionType) =>
            resolver =>
            {
                var constructor = ResolveConstructor(concretionType);
                var parameters = ResolveParameters(constructor.GetParameters(), resolver);

                var instance = constructor.Invoke(parameters);
                Inject(instance);
                return instance;
            };

        private object[] ResolveParameters(IEnumerable<ParameterInfo> parameters, IResolver resolver) =>
            parameters
                .Select(x => ResolveParameter(x, resolver))
                .ToArray();

        private object ResolveParameter(ParameterInfo parameter, IResolver resolver)
        {
            _logger?.Log($"Resolving parameter {parameter.Name}...");
            var attribute = parameter.GetAttribute<InjectAttribute>();
            var isOptional = (attribute?.IsOptional ?? false) || parameter.IsOptional;
            return resolver.Resolve(parameter.ParameterType, attribute?.Id, isOptional);
        }

        private ConstructorInfo ResolveConstructor(Type type)
        {
            var constructors = type.GetConstructors();
            
            if (constructors.Length == 1)
                return constructors.First();
            
            var constructor = constructors.FirstOrDefault(x => x.HasAttribute<InjectAttribute>());
            if (constructor == null)
                throw new InvalidOperationException(
                    $"Type {type.Name} has multiple constructors, but none of them is marked with an injection attribute to make it the default.");

            return constructor;
        }

        #endregion

        #region IInjector members

        public void Inject(object obj, IResolver resolver = null)
        {
            resolver = resolver ?? this;
            
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            
            InjectFieldsAndProperties(obj, resolver);
            InjectMethods(obj, resolver);
        }

        public void Inject(IEnumerable<object> objects, IResolver resolver = null)
        {
            var list = objects.ToList();
            list.ForEach(x => InjectFieldsAndProperties(x, resolver));
            list.ForEach(x => InjectMethods(x, resolver));
        }

        private void InjectFieldsAndProperties(object obj, IResolver resolver)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            
            if (obj is GameObject)
            {
                InjectGameObjectFieldsAndProperties((GameObject) obj, resolver);
                return;
            }
            
            obj.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .ForEach(field => InjectField(obj, field, resolver));

            obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .ForEach(property => InjectProperty(obj, property, resolver));
        }

        private void InjectMethods(object obj, IResolver resolver)
        {
            if (obj is GameObject)
            {
                InjectGameObjectMethods((GameObject) obj, resolver);
                return;
            }
            
            obj.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(method => method.HasAttribute<InjectAttribute>())
                .ForEach(method => InjectMethod(obj, method, resolver));            
        }

        private bool IsValidComponent(MonoBehaviour behaviour)
        {
            if (behaviour != null)
                return true;
            
            _logger?.LogWarning(nameof(Container), "Skipping null MonoBehaviour.");
            return false;
        }
        
        private void InjectGameObjectFieldsAndProperties(GameObject go, IResolver resolver)
        {
            go.GetComponents<MonoBehaviour>()
                .Where(IsValidComponent)
                .ForEach(component => InjectFieldsAndProperties(component, resolver));
            
            go.Children()
                .ForEach(child => InjectGameObjectFieldsAndProperties(child, resolver));
        }

        private void InjectGameObjectMethods(GameObject go, IResolver resolver)
        {
            go.GetComponents<MonoBehaviour>()
                .Where(IsValidComponent)
                .ForEach(component => InjectMethods(component, resolver));
            
            go.Children()
                .ForEach(child => InjectGameObjectMethods(child, resolver));
        }

        private void InjectProperty(object obj, PropertyInfo property, IResolver resolver)
        {
            var inject = property.GetAttribute<InjectAttribute>();
            if (inject == null)
                return;
            
            var value = resolver.Resolve(property.PropertyType, inject.Id, inject.IsOptional);
            _logger?.Log($"Injecting {obj.GetType().Name}.{property.Name} ({property.PropertyType.Name}) <= {FormatValue(value)}");
            property.SetValue(obj, value, null);
        }

        private void InjectField(object obj, FieldInfo field, IResolver resolver)
        {
            var inject = field.GetAttribute<InjectAttribute>();
            if (inject == null)
                return;
            
            var value = resolver.Resolve(field.FieldType, inject.Id, inject.IsOptional);
            _logger?.Log($"Injecting {obj.GetType().Name}.{field.Name} ({field.FieldType.Name}) <= {FormatValue(value)}");
            field.SetValue(obj, value);
        }

        private void InjectMethod(object obj, MethodInfo method, IResolver resolver)
        {
            var inject = method.GetAttribute<InjectAttribute>();
            if (inject == null)
                return;
            
            var parameters = ResolveParameters(method.GetParameters(), resolver);
            _logger?.Log($"Injecting {obj.GetType().Name}.{method.Name}({FormatParameters(parameters)})");
            method.Invoke(obj, parameters);
        }

        private static string FormatParameters(object[] parameters) =>
            parameters.Select(FormatValue).ToDelimitedString(", ");

        private static string FormatValue(object value) =>
            value?.ToString() ?? "null";

        #endregion

        #region IDisposable members

        public void Dispose()
        {
            _bindings
                .Select(x => x.Instance)
                .OfType<IDisposable>()
                .ForEach(x => x.Dispose());
        }

        #endregion        
    }
}