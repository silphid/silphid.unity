using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IReflector _reflector;
        private readonly ILogger _logger;
        private static readonly Func<IResolver, object> NullFactory = null;

        #endregion

        #region Constructors

        public Container(IReflector reflector, ILogger logger = null)
        {
            _reflector = reflector;
            _logger = logger;
        }

        #endregion

        #region IContainer members

        public IContainer Create() =>
            new Container(_reflector, _logger);

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
            if (abstractionType == null)
                throw new ArgumentNullException(nameof(abstractionType));
            
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            
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

        public Func<IResolver, object> ResolveFactory(Type abstractionType, string id = null)
        {
            _logger?.Log($"Resolving {abstractionType.Name}...");

            abstractionType = ResolveForward(abstractionType);

            return ResolveFromTypeMappings(abstractionType, id) ??
                   ResolveFromListMappings(abstractionType) ??
                   ThrowUnresolvedType(abstractionType, id);
        }

        private Type ResolveForward(Type abstractionType) =>
            _forwards.GetValueOrDefault(abstractionType) ?? abstractionType;

        private Func<IResolver, object> ThrowUnresolvedType(Type abstractionType, string id)
        {
            throw new UnresolvedTypeException(abstractionType, id);
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
                var typeInfo = GetTypeInfo(concretionType);
                if (typeInfo.Constructor.ConstructorException != null)
                    throw typeInfo.Constructor.ConstructorException;
                
                var parameters = ResolveParameters(typeInfo.Constructor.Parameters, resolver);

                var instance = typeInfo.Constructor.Constructor.Invoke(parameters);
                Inject(instance, resolver);
                return instance;
            };

        private object[] ResolveParameters(IEnumerable<InjectParameterInfo> parameters, IResolver resolver) =>
            parameters
                .Select(x => ResolveParameter(x, resolver))
                .ToArray();

        private object ResolveParameter(InjectParameterInfo parameter, IResolver resolver)
        {
            _logger?.Log($"Resolving parameter {parameter.Name}");
            try
            {
                return resolver.Resolve(parameter.Type, parameter.Id);
            }
            catch (UnresolvedTypeException)
            {
                _logger?.Log($"Falling back to default value: {parameter.DefaultValue}");
                return parameter.DefaultValue;
            }
        }

        #endregion

        #region IInjector members

        public void Inject(object obj, IResolver resolver = null)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            
            resolver = resolver ?? this;
            
            var typeInfo = GetObjectTypeInfo(obj);
            
            InjectFieldsAndProperties(obj, typeInfo, resolver);
            InjectMethods(obj, typeInfo, resolver);
        }

        public void Inject(IEnumerable<object> objects, IResolver resolver = null)
        {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            resolver = resolver ?? this;
            
            var list = objects.ToArray();
            var typeInfos = list.Select(GetObjectTypeInfo).ToArray();
            
            list.ForEach((i, x) => InjectFieldsAndProperties(x, typeInfos[i], resolver));
            list.ForEach((i, x) => InjectMethods(x, typeInfos[i], resolver));
        }

        private void InjectFieldsAndProperties(object obj, InjectTypeInfo typeInfo, IResolver resolver)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            
            if (obj is GameObject)
            {
                InjectGameObjectFieldsAndProperties((GameObject) obj, resolver);
                return;
            }

            typeInfo.FieldsAndProperties.ForEach(x => InjectFieldOrProperty(obj, x, resolver));
        }

        private void InjectMethods(object obj, InjectTypeInfo typeInfo, IResolver resolver)
        {
            if (obj is GameObject)
            {
                InjectGameObjectMethods((GameObject) obj, resolver);
                return;
            }
            
            typeInfo.Methods.ForEach(method => InjectMethod(obj, method, resolver));            
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
                .ForEach(component => InjectFieldsAndProperties(component, GetObjectTypeInfo(component), resolver));
            
            go.Children()
                .ForEach(child => InjectGameObjectFieldsAndProperties(child, resolver));
        }

        private void InjectGameObjectMethods(GameObject go, IResolver resolver)
        {
            go.GetComponents<MonoBehaviour>()
                .Where(IsValidComponent)
                .ForEach(component => InjectMethods(component, GetObjectTypeInfo(component), resolver));
            
            go.Children()
                .ForEach(child => InjectGameObjectMethods(child, resolver));
        }

        private void InjectFieldOrProperty(object obj, InjectFieldOrPropertyInfo member, IResolver resolver)
        {
            try
            {
                var value = resolver.Resolve(member.Type, member.Id);
                _logger?.Log($"Injecting {obj.GetType().Name}.{member.Name} ({member.Name}) <= {FormatValue(value)}");
                member.SetValue(obj, value);
            }
            catch (UnresolvedTypeException)
            {
                if (!member.IsOptional)
                    throw;
            }
        }

        private void InjectMethod(object obj, InjectMethodInfo method, IResolver resolver)
        {
            var parameters = ResolveParameters(method.Parameters, resolver);
            _logger?.Log($"Injecting {obj.GetType().Name}.{method.Name}({FormatParameters(parameters)})");
            method.Method.Invoke(obj, parameters);
        }

        #endregion

        #region Helpers

        private InjectTypeInfo GetTypeInfo(Type type) =>
            _reflector.GetTypeInfo(type);
        
        private InjectTypeInfo GetObjectTypeInfo(object obj) =>
            _reflector.GetTypeInfo(obj.GetType());
        
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