using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Injexit
{
    public class Container : IContainer
    {
        public static readonly IContainer Null = new NullContainer();
        private static readonly ILog Log = LogManager.GetLogger(typeof(Container));
        
        #region Private fields

        private readonly List<Binding> _bindings = new List<Binding>();
        private readonly IReflector _reflector;
        private static readonly Func<IResolver, object> NullFactory = null;

        #endregion

        #region Constructors

        public Container(IReflector reflector)
        {
            _reflector = reflector;
        }

        #endregion

        #region IContainer members

        public IContainer Create() =>
            new Container(_reflector);

        #endregion
        
        #region IBinder members

        public IBinding BindReference(Type abstractionType, string reference)
        {
            if (reference == null)
                throw new ArgumentNullException(nameof(reference));

            var binding = new Binding(this, abstractionType, reference);
            _bindings.Add(binding);
            return binding;
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

        public Func<IResolver, object> ResolveFactory(Type abstractionType, string name = null)
        {
            Log.Debug($"Resolving {abstractionType.Name}");

            return ResolveFromTypeMappings(abstractionType, name) ??
                   ResolveFromListMappings(abstractionType, name) ??
                   ThrowUnresolvedType(abstractionType, name);
        }

        private Func<IResolver, object> ThrowUnresolvedType(Type abstractionType, string id)
        {
            throw new UnresolvedTypeException(abstractionType, id);
        }

        private Func<IResolver, object> ResolveFromListMappings(Type abstractionType, string id)
        {
            var elementType = GetListElementType(abstractionType);
            if (elementType == null)
                return NullFactory;

            var factories = GetListFactories(elementType, id);
            if (factories.Count == 0)
                return NullFactory;
            
            // Array or Enumerable<T>
            if (IsArrayOrGenericEnumerable(abstractionType))
                return resolver => ToTypedArray(factories.Select(x => x(resolver)), elementType);

            // List<T>
            return resolver => ToTypedList(factories.Select(x => x(resolver)), elementType);
        }

        private bool IsArrayOrGenericEnumerable(Type type) =>
            type.IsArray || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);

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

        private List<Func<IResolver, object>> GetListFactories(Type abstractionType, string name) =>
            _bindings
                .Where(x => x.AbstractionType == abstractionType && x.InList && (x.Name == null || x.Name == name))
                .Select(GetFactory)
                .ToList();

        private Func<IResolver, object> ResolveFromTypeMappings(Type abstractionType, string name) =>
            GetFactory(ResolveType(abstractionType, name));

        private Binding ResolveType(Type abstractionType, string name)
        {
            var binding = _bindings.FirstOrDefault(x =>
                x.AbstractionType == abstractionType &&
                (x.Name == null || x.Name == name));
            
            if (binding != null)
                Log.Debug($"Resolved {binding}");

            return binding;
        }

        private Func<IResolver, object> GetFactory(Binding binding)
        {
            if (binding == null)
                return NullFactory;

            if (binding.Reference != null)
            {
                var aliasBinding = _bindings.FirstOrDefault(x => x.Id == binding.Reference);
                if (aliasBinding == null)
                    throw new InvalidOperationException($"Failed to resolve reference to alias {binding.Reference}");
                
                return GetFactory(aliasBinding);
            }
            
            if (binding.Lifetime == Lifetime.Transient)
                return GetFactory(binding.ConcretionType, binding.OverrideResolver);

            return resolver =>
                binding.Instance
                ?? (binding.Instance = GetFactory(binding.ConcretionType, binding.OverrideResolver).Invoke(resolver));
        }

        private Func<IResolver, object> GetFactory(Type concretionType, IResolver overrideResolver)
        {
            var factory = GetFactory(concretionType);
            return factory != null
                ? resolver => factory(resolver.Using(overrideResolver))
                : NullFactory;
        }

        private Func<IResolver, object> GetFactory(Type concretionType) =>
            resolver =>
            {
                var typeInfo = GetTypeInfo(concretionType);
                if (typeInfo.Constructor.ConstructorException != null)
                    throw typeInfo.Constructor.ConstructorException;
                
                var parameters = ResolveParameters(concretionType, typeInfo.Constructor.Parameters, resolver);

                var instance = typeInfo.Constructor.Constructor.Invoke(parameters);
                Inject(instance, resolver);
                return instance;
            };

        private object[] ResolveParameters(Type dependentType, IEnumerable<InjectParameterInfo> parameters, IResolver resolver) =>
            parameters
                .Select(x => ResolveParameter(dependentType, x, resolver))
                .ToArray();

        private object ResolveParameter(Type dependentType, InjectParameterInfo parameter, IResolver resolver)
        {
            Log.Debug($"Resolving parameter {parameter.Name}");
            try
            {
                return resolver.Resolve(parameter.Type, parameter.CanonicalName);
            }
            catch (UnresolvedDependencyException ex)
            {
                if (!parameter.IsOptional)
                    throw new UnresolvedDependencyException(dependentType, ex, ex.MemberName);
            }
            catch (UnresolvedTypeException ex)
            {
                if (!parameter.IsOptional)
                    throw new UnresolvedDependencyException(dependentType, ex, parameter.Name);
            }

            Log.Debug($"Falling back to default value: {parameter.DefaultValue}");
            return parameter.DefaultValue;
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
            
            Log.Warn("Skipping null MonoBehaviour.");
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
                var value = resolver.Resolve(member.Type, member.CanonicalName);
                Log.Debug($"Injecting {obj.GetType().Name}.{member.Name} ({member.Name}) <= {FormatValue(value)}");
                member.SetValue(obj, value);
            }
            catch (UnresolvedTypeException ex)
            {
                if (!member.IsOptional)
                    throw new UnresolvedDependencyException(obj.GetType(), ex, member.Name);
            }
        }

        private void InjectMethod(object obj, InjectMethodInfo method, IResolver resolver)
        {
            var parameters = ResolveParameters(obj.GetType(), method.Parameters, resolver);
            Log.Debug($"Injecting {obj.GetType().Name}.{method.Name}({FormatParameters(parameters)})");
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

        /// <inheritdoc/>
        public void InstantiateEagerSingles()
        {
            _bindings
                .Where(x => x.Lifetime == Lifetime.EagerSingle && x.Instance == null)
                .ForEach(x => this.Resolve(x.AbstractionType, x.Name));
        }

        #endregion        
    }
}