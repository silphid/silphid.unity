using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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

        private readonly IReflector _reflector;
        private readonly Func<object, bool> _injectionPredicate;
        private readonly List<Binding> _bindings = new List<Binding>();
        private int _recursionDepth;
        private bool _isDisposed;

        #endregion

        #region Constructors

        public Container(IReflector reflector, Func<object, bool> injectionPredicate = null)
        {
            _reflector = reflector;
            _injectionPredicate = injectionPredicate;
        }

        #endregion

        #region IContainer members

        public IContainer Create() =>
            new Container(_reflector, _injectionPredicate);

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
            var binding = BindInstanceInternal(abstractionType, instance);

            if (abstractionType.HasAttribute<KnownTypeAttribute>())
                Log.Warn(
                    $"KnownTypeAttribute on base class {abstractionType.Name} has been deprecated in favor of ExtraBindAttribute on derived classes.");

            instance.GetType()
                    .GetAttributes<ExtraBindAttribute>()
                    .Select(x => x.Type)
                    .Where(x => x != abstractionType)
                    .ForEach(x => BindInstanceInternal(x, instance));

            return binding;
        }

        private Binding BindInstanceInternal(Type abstractionType, object instance)
        {
            if (abstractionType == null)
                throw new ArgumentNullException(nameof(abstractionType));

            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var instanceType = instance.GetType();

            if (!instanceType.IsAssignableTo(abstractionType))
                throw new InvalidOperationException(
                    $"Instance type {instanceType.Name} must be assignable to abstraction type {abstractionType.Name}.");

            var binding = new Binding(this, abstractionType, instanceType)
            {
                Instance = instance,
                Lifetime = Lifetime.Single
            };

            _bindings.Add(binding);
            return binding;
        }

        public IBinding Bind(Type abstractionType, Type concretionType)
        {
            if (!concretionType.IsAssignableTo(abstractionType))
                throw new InvalidOperationException(
                    $"Concretion type {concretionType.Name} must be assignable to abstraction type {abstractionType.Name}.");

            if (concretionType.IsAbstract)
                throw new InvalidOperationException($"Concretion type {concretionType.Name} cannot be abstract.");

            var binding = new Binding(this, abstractionType, concretionType);
            _bindings.Add(binding);

            return binding;
        }

        public IBinding<T> Bind<T>(Type abstractionType, Func<IResolver, T> selector)
        {
            var binding = new Binding(this, abstractionType, x => selector(x));
            _bindings.Add(binding);
            return binding.Typed<T>();
        }

        #endregion

        #region IResolver members

        public Result ResolveResult(Type abstractionType, Type dependentType, string name = null)
        {
            _recursionDepth++;

            try
            {
                if (_recursionDepth > MaxRecursionDepth)
                    throw new CircularDependencyException(abstractionType);

                try
                {
                    return ResolveFromTypeMappings(abstractionType, dependentType, name) ??
                           ResolveFromListMappings(abstractionType, dependentType, name) ??
                           GetUnresolvedDependencyResult(abstractionType, dependentType, name);
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

        private Result  GetUnresolvedDependencyResult(Type abstractionType, Type dependentType, string name) =>
            new Result(new DependencyException(abstractionType, dependentType, name, "Binding not found", this));

        private Result? ResolveFromListMappings(Type abstractionType, Type dependentType, string name)
        {
            var elementType = GetListElementType(abstractionType);
            if (elementType == null)
                return null;

            var factories = GetListFactories(elementType, dependentType, name);
            if (factories.Count == 0)
                return null;

            // Array or Enumerable<T>
            if (IsArrayOrGenericEnumerable(abstractionType))
                return new Result(
                    resolver => ToTypedArray(
                        factories.Select(x => x.ResolveInstance(resolver))
                                 .ToArray(),
                        elementType),
                    null);

            // List<T>
            return new Result(
                resolver => ToTypedList(
                    factories.Select(x => x.ResolveInstance(resolver))
                             .ToArray(),
                    elementType),
                null);
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
                return abstractionType.GetGenericArguments()
                                      .First();

            return null;
        }

        private List<Result> GetListFactories(Type abstractionType, Type dependentType, string name) =>
            _bindings.Where(
                          x => x.AbstractionType == abstractionType && x.InList &&
                               (x.Name == null || string.Equals(
                                    x.Name,
                                    name,
                                    StringComparison.CurrentCultureIgnoreCase)))
                     .Select(x => GetFactoryForBinding(x, dependentType, name))
                     .ToList();

        private Result? ResolveFromTypeMappings(Type abstractionType, Type dependentType, string name)
        {
            var binding = ResolveBindingForType(abstractionType, dependentType, name);
            if (binding == null)
                return null;

            return GetFactoryForBinding(binding, dependentType, name);
        }

        private Binding ResolveBindingForType(Type abstractionType, Type dependentType, string name)
        {
            var binding = _bindings.FirstOrDefault(
                x => x.AbstractionType == abstractionType && (x.Name == null || string.Equals(
                                                                  x.Name,
                                                                  name,
                                                                  StringComparison.InvariantCultureIgnoreCase)));

            if (binding != null && Log.IsDebugEnabled)
                Log.Debug($"Resolved {binding}");

            return binding;
        }

        private Result GetFactoryForBinding(Binding binding, Type dependentType, string name)
        {
            if (binding.Reference != null)
                return GetFactoryForReferenceBinding(binding, dependentType, name);

            if (binding.ConcretionType != null)
            {
                if (binding.Lifetime == Lifetime.Transient || binding.Lifetime == Lifetime.Scoped)
                    return GetFactoryForConcretion(
                        binding.ConcretionType,
                        binding.OverrideResolver,
                        binding.IsOverrideResolverRecursive,
                        binding.Decoration,
                        binding.Scope,
                        dependentType,
                        name);

                return new Result(
                    resolver => binding.Instance ?? (binding.Instance = GetFactoryForConcretion(
                                                             binding.ConcretionType,
                                                             binding.OverrideResolver,
                                                             binding.IsOverrideResolverRecursive,
                                                             binding.Decoration,
                                                             binding.Scope,
                                                             dependentType,
                                                             name)
                                                        .ResolveInstance(resolver.BaseResolver)),
                    binding.Scope);
            }

            if (binding.Factory != null)
            {
                if (binding.Lifetime == Lifetime.Transient || binding.Lifetime == Lifetime.Scoped)
                    return new Result(binding.Factory, binding.Scope);

                return new Result(
                    resolver => binding.Instance ?? (binding.Instance = binding.Factory(resolver.BaseResolver)),
                    binding.Scope);
            }

            throw new NotSupportedException("Binding must specify one of ConcretionType or Selector");
        }

        private Result GetFactoryForReferenceBinding(Binding binding, Type dependentType, string name)
        {
            var referenceBinding = binding.Reference.Binding;
            if (referenceBinding == null)
                return new Result(
                    new DependencyException(
                        binding.AbstractionType,
                        dependentType,
                        name,
                        $"No binding associated with Id for abstraction type {binding.AbstractionType.Name}. " +
                        "You are missing a call to .Id(...)",
                        this));

            if (!referenceBinding.ConcretionType.IsAssignableTo(binding.AbstractionType))
                return new Result(
                    new DependencyException(
                        binding.AbstractionType,
                        dependentType,
                        name,
                        $"BindingId concrete type {referenceBinding.ConcretionType.Name} is not " +
                        $"assignable to Reference abstraction type {binding.AbstractionType.Name}",
                        this));

            if (Log.IsDebugEnabled)
                Log.Debug($"Resolved &{binding.Reference} to {referenceBinding}");

            return GetFactoryForBinding(referenceBinding, dependentType, name);
        }

        private Result GetFactoryForConcretion(Type concretionType,
                                               IResolver overrideResolver,
                                               bool isRecursive,
                                               Func<IResolver, object, object> decoration,
                                               IScope scope,
                                               Type dependentType,
                                               string name)
        {
            var factory = GetFactoryForConcretion(concretionType, decoration);
            return new Result(
                resolver =>
                {
                    try
                    {
                        return factory(resolver.Using(overrideResolver, isRecursive));
                    }
                    catch (DependencyException ex)
                    {
                        // TODO: Temporary work-around to prevent an extra level of exception wrapping
                        if (ex.DependentTypes.LastOrDefault() == concretionType)
                            throw;

                        throw ex.WithDependent(concretionType);
                    }
                },
                scope);
        }

        private Func<IResolver, object> GetFactoryForConcretion(Type concretionType,
                                                                Func<IResolver, object, object> decoration) =>
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

                        if (decoration != null)
                            instance = decoration(resolver, instance);

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

        private object[] ResolveParameters(Type dependentType,
                                           IEnumerable<InjectParameterInfo> parameters,
                                           IResolver resolver) =>
            parameters.Select(x => ResolveParameter(dependentType, x, resolver))
                      .ToArray();

        private object ResolveParameter(Type dependentType, InjectParameterInfo parameter, IResolver resolver)
        {
            if (Log.IsDebugEnabled)
                Log.Debug($"Resolving parameter {parameter.Name}");

            var result = resolver.ResolveResult(parameter.Type, dependentType, parameter.CanonicalName);

            if (result.Exception != null)
            {
                if (parameter.IsOptional)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug($"Falling back to default value: {parameter.DefaultValue}");

                    return parameter.DefaultValue;
                }

                var ex = result.Exception as DependencyException;
                if (ex != null)
                    throw ex;

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
            var typeInfos = list.Select(GetObjectTypeInfo)
                                .ToArray();

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
            if (behaviour == null)
            {
                if (Log.IsWarnEnabled)
                    Log.Warn("Skipping null MonoBehaviour.");

                return false;
            }

            return _injectionPredicate?.Invoke(behaviour) ?? true;
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
            var result = resolver.ResolveResult(member.Type, obj.GetType(), member.CanonicalName);
            if (result.Exception != null)
            {
                if (!member.IsOptional)
                    throw result.Exception;

                return;
            }

            var value = result.ResolveInstance(resolver);

            if (Log.IsDebugEnabled)
                Log.Debug($"Injecting {obj.GetType().Name}.{member.Name} = {FormatValue(value)}");

            member.SetValue(obj, value);
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
            parameters.Select(FormatValue)
                      .JoinAsString(", ");

        private static string FormatValue(object value) =>
            value?.ToString() ?? "null";

        #endregion

        #region IDisposable members

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _bindings.Select(x => x.Instance)
                     .OfType<IDisposable>()
                     .ForEach(x => x.Dispose());
        }

        public void InstantiateEagerSingles(IResolver resolver)
        {
            _bindings.Where(x => x.Lifetime == Lifetime.EagerSingle && x.Instance == null)
                     .ForEach(x => resolver.Resolve(x.AbstractionType, null, x.Name));
        }

        #endregion

        public override string ToString() =>
            _bindings.JoinAsString("\r\n");
    }
}