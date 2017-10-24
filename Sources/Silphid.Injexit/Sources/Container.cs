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
        public const int MaxRecursionDepth = 15;
        public static readonly IContainer Null = new NullContainer();
        private static readonly ILog Log = LogManager.GetLogger(typeof(Container));
        
        #region Private fields

        private readonly List<Binding> _bindings = new List<Binding>();
        private readonly IReflector _reflector;
        private int _recursionDepth;

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

        public IBinding BindReference(Type abstractionType, BindingId id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            
            var binding = new Binding(this, abstractionType, id);
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

        public Result ResolveResult(Type abstractionType, string name = null)
        {
            _recursionDepth++;
            
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug($"Resolving {abstractionType.Name}");
                
                if (_recursionDepth > MaxRecursionDepth)
                    throw new CircularDependencyException(abstractionType);

                try
                {
                    return ResolveFromTypeMappings(abstractionType, name) ??
                           ResolveFromListMappings(abstractionType, name) ??
                           ThrowUnresolvedType(abstractionType, name);
                }
                catch (CircularDependencyException ex)
                {
                    throw new CircularDependencyException(abstractionType, ex);
                }
            }
            finally
            {
                _recursionDepth--;
            }
        }

        public IResolver BaseResolver => this;

        private Result ThrowUnresolvedType(Type abstractionType, string name) =>
            new Result(new UnresolvedTypeException(abstractionType, name));

        private Result? ResolveFromListMappings(Type abstractionType, string name)
        {
            var elementType = GetListElementType(abstractionType);
            if (elementType == null)
                return null;

            var factories = GetListFactories(elementType, name);
            if (factories.Count == 0)
                return null;
            
            // Array or Enumerable<T>
            if (IsArrayOrGenericEnumerable(abstractionType))
                return new Result(resolver =>
                    ToTypedArray(factories.Select(x => x.ResolveInstance(resolver)), elementType));

            // List<T>
            return new Result(resolver =>
                ToTypedList(factories.Select(x => x.ResolveInstance(resolver)), elementType));
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

        private List<Result> GetListFactories(Type abstractionType, string name) =>
            _bindings
                .Where(x => x.AbstractionType == abstractionType && x.InList && (x.Name == null || x.Name == name))
                .Select(x => GetFactoryForBinding(x, name))
                .ToList();

        private Result? ResolveFromTypeMappings(Type abstractionType, string name)
        {
            var binding = ResolveBindingForType(abstractionType, name);
            if (binding == null)
                return null;
            
            return GetFactoryForBinding(binding, name);
        }

        private Binding ResolveBindingForType(Type abstractionType, string name)
        {
            var binding = _bindings.FirstOrDefault(x =>
                x.AbstractionType == abstractionType &&
                (x.Name == null || x.Name == name));
            
            if (binding != null && Log.IsDebugEnabled)
                Log.Debug($"Resolved {binding}");
            
            return binding;
        }

        private Result GetFactoryForBinding(Binding binding, string name)
        {
            if (binding.Reference != null)
            {
                var referenceBinding = binding.Reference.Binding;
                if (referenceBinding == null)
                    return new Result(
                        new UnresolvedTypeException(binding.AbstractionType, null, $"No binding bound to {binding.Reference}"));
                
                if (!referenceBinding.ConcretionType.IsAssignableTo(binding.AbstractionType))
                    return new Result(
                        new UnresolvedTypeException(binding.AbstractionType, null, $"Binding {binding.Reference} concrete type {referenceBinding.ConcretionType.Name} is not assignable to Reference abstraction type {binding.AbstractionType.Name}"));

                if (Log.IsDebugEnabled)
                    Log.Debug($"Resolved &{binding.Reference} to {referenceBinding}");
                
                return GetFactoryForBinding(referenceBinding, name);
            }
            
            if (binding.Lifetime == Lifetime.Transient)
                return GetFactoryForConcretion(binding.ConcretionType, binding.OverrideResolver, binding.IsOverrideResolverRecursive, name);

            return new Result(resolver =>
                binding.Instance
                ?? (binding.Instance = GetFactoryForConcretion(
                        binding.ConcretionType,
                        binding.OverrideResolver,
                        binding.IsOverrideResolverRecursive,
                        name)
                    .ResolveInstance(resolver.BaseResolver)));
        }

        private Result GetFactoryForConcretion(Type concretionType, IResolver overrideResolver, bool isRecursive, string name)
        {
            var factory = GetFactoryForConcretion(concretionType);
            return new Result(resolver =>
            {
                try
                {
                    return factory(resolver.Using(overrideResolver, isRecursive));
                }
                catch (UnresolvedDependencyException ex)
                {
                    // TODO: Temporary work-around to prevent an extra level of exception wrapping
                    if (ex.AncestorTypes.LastOrDefault() == concretionType)
                        throw;
                    
                    throw new UnresolvedDependencyException(concretionType, ex, name);
                }
            });
        }

        private Func<IResolver, object> GetFactoryForConcretion(Type concretionType) =>
            resolver =>
            {
                _recursionDepth++;
            
                try
                {
                    if (_recursionDepth > MaxRecursionDepth)
                        throw new CircularDependencyException(concretionType);

                    try
                    {
                        var typeInfo = GetTypeInfo(concretionType);
                        if (typeInfo.Constructor.ConstructorException != null)
                            throw typeInfo.Constructor.ConstructorException;

                        var parameters = ResolveParameters(concretionType, typeInfo.Constructor.Parameters, resolver);

                        var instance = typeInfo.Constructor.Constructor.Invoke(parameters);
                        Inject(instance, resolver);
                        return instance;
                    }
                    catch (CircularDependencyException ex)
                    {
                        throw new CircularDependencyException(concretionType, ex);
                    }
                }
                finally
                {
                    _recursionDepth--;
                }
            };

        private object[] ResolveParameters(Type dependentType, IEnumerable<InjectParameterInfo> parameters, IResolver resolver) =>
            parameters
                .Select(x => ResolveParameter(dependentType, x, resolver))
                .ToArray();

        private object ResolveParameter(Type dependentType, InjectParameterInfo parameter, IResolver resolver)
        {
            if (Log.IsDebugEnabled)
                Log.Debug($"Resolving parameter {parameter.Name}");
            
            var result = resolver.ResolveResult(parameter.Type, parameter.CanonicalName);

            if (result.Exception != null)
            {
                if (parameter.IsOptional)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug($"Falling back to default value: {parameter.DefaultValue}");
            
                    return parameter.DefaultValue;
                }

                var depEx = result.Exception as UnresolvedDependencyException;
                if (depEx != null)
                    throw new UnresolvedDependencyException(dependentType, depEx, depEx.Name);

                var typeEx = result.Exception as UnresolvedTypeException;
                if (typeEx != null)
                    throw new UnresolvedDependencyException(dependentType, typeEx, parameter.Name);

                throw result.Exception;
            }

            return result.ResolveInstance(resolver);
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
            
            if (Log.IsWarnEnabled)
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
                
                if (Log.IsDebugEnabled)
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
            
            if (Log.IsDebugEnabled)
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