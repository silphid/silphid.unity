using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silphid.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Silphid.Showzup.Injection
{
    public class Container : IBinder, IResolver
    {
        #region Lifetime enum

        private enum Lifetime
        {
            /// <summary>
            /// A new object is created for each individual usage.
            /// </summary>
            Transient,

            /// <summary>
            /// A single instance is shared for all usages (and it is lazily created, if it was not specified at bind-time).
            /// </summary>
            Single
        }
        
        #endregion

        #region Mapping inner class

        private class Mapping
        {
            public Type AbstractionType { get; }
            public Type ConcretionType { get; }
            public Lifetime Lifetime { get; }
            public IResolver OverrideResolver { get; }
            public object Instance { get; set; }
            public bool IsList { get; }

            public Mapping(Type abstractionType, Type concretionType, Lifetime lifetime, IResolver overrideResolver, bool isList)
            {
                AbstractionType = abstractionType;
                ConcretionType = concretionType;
                Lifetime = lifetime;
                OverrideResolver = overrideResolver;
                IsList = isList;
            }
        }

        #endregion
        
        #region Private fields

        private readonly List<Mapping> _mappings = new List<Mapping>();
        private readonly ILogger _logger;
        private static readonly Func<IResolver, object> NullFactory = null;

        #endregion

        #region Constructors

        public Container(ILogger logger = null)
        {
            _logger = logger;
        }

        #endregion
        
        #region Binding

        public void BindInstance(Type abstractionType, object instance) =>
            BindInstanceInternal(abstractionType, instance, isList: false);

        private void BindInstanceInternal(Type abstractionType, object instance, bool isList)
        {
            if (!instance.GetType().IsAssignableTo(abstractionType))
                throw new InvalidOperationException($"Instance type {instance.GetType().Name} must be assignable to abstraction type {abstractionType.Name}.");

            _mappings.Add(
                new Mapping(abstractionType, instance.GetType(), Lifetime.Single, null, isList)
                {
                    Instance = instance
                });
            
            abstractionType.GetAttributes<BindAttribute>()
                .Select(x => x.Type)
                .ForEach(x => BindInstance(x, instance));
        }

        public void BindInstanceAsList(Type abstractionType, object instance) =>
            BindInstanceInternal(abstractionType, instance, isList: true);

        public void Bind(Type abstractionType, Type concretionType, IResolver overrideResolver = null) =>
            BindInternal(abstractionType, concretionType, overrideResolver, isList: false);

        public void BindInternal(Type abstractionType, Type concretionType, IResolver overrideResolver, bool isList)
        {
            if (!concretionType.IsAssignableTo(abstractionType))
                throw new InvalidOperationException($"Concretion type {concretionType.Name} must be assignable to abstraction type {abstractionType.Name}.");

            _mappings.Add(new Mapping(abstractionType, concretionType, Lifetime.Transient, overrideResolver, isList));
        }

        public void BindAsList(Type abstractionType, Type concretionType, IResolver overrideResolver = null) =>
            BindInternal(abstractionType, concretionType, overrideResolver, isList: true);

        public void BindSingle(Type abstractionType, Type concretionType, IResolver overrideResolver = null) =>
            BindSingleInternal(abstractionType, concretionType, overrideResolver, isList: false);

        private void BindSingleInternal(Type abstractionType, Type concretionType, IResolver overrideResolver, bool isList) =>
            _mappings.Add(new Mapping(abstractionType, concretionType, Lifetime.Single, overrideResolver, isList));


        public void BindSingleAsList(Type abstractionType, Type concretionType, IResolver overrideResolver = null) =>
            BindSingleInternal(abstractionType, concretionType, overrideResolver, isList: true);

        #endregion
        
        #region Resolve

        public Func<IResolver, object> ResolveFactory(Type abstractionType, bool isOptional = false, bool isFallbackToSelfBinding = true) =>
            ResolveFromTypeMappings(abstractionType) ??
            ResolveFromListMappings(abstractionType) ??
            ResolveSelfBinding(abstractionType, isFallbackToSelfBinding) ??
            ThrowIfNotOptional(abstractionType, isOptional);

        private Func<IResolver, object> ThrowIfNotOptional(Type abstractionType, bool isOptional)
        {
            if (!isOptional)
                throw new Exception($"No mapping for required type {abstractionType.Name}.");

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
            _mappings
                .Where(x => x.AbstractionType == abstractionType && x.IsList)
                .Select(ResolveFactoryInternal)
                .ToList();

        private Func<IResolver, object> ResolveFromTypeMappings(Type abstractType) =>
            ResolveFactoryInternal(ResolveType(abstractType));

        private Mapping ResolveType(Type abstractType) =>
            _mappings.FirstOrDefault(x => x.AbstractionType == abstractType);

        private Func<IResolver, object> ResolveSelfBinding(Type abstractionType, bool isSelfBindingAllowed) =>
            isSelfBindingAllowed && !abstractionType.IsAbstract
                ? ResolveFactoryInternal(abstractionType)
                : NullFactory;

        private Func<IResolver, object> ResolveFactoryInternal(Mapping mapping)
        {
            if (mapping == null)
                return NullFactory;
            
            if (mapping.Lifetime == Lifetime.Transient)
                return ResolveFactoryInternal(mapping.ConcretionType, mapping.OverrideResolver);

            return resolver =>
                mapping.Instance
                ?? (mapping.Instance = ResolveFactoryInternal(mapping.ConcretionType, mapping.OverrideResolver).Invoke(resolver));
        }

        private Func<IResolver, object> ResolveFactoryInternal(Type concretionType, IResolver overrideResolver)
        {
            var factory = ResolveFactoryInternal(concretionType);
            return factory != null
                ? resolver => factory(resolver.WithOverride(overrideResolver))
                : NullFactory;
        }

        private Func<IResolver, object> ResolveFactoryInternal(Type concretionType) =>
            resolver =>
            {
                var constructor = ResolveConstructor(concretionType);
                var parameters = ResolveParameters(constructor.GetParameters(), resolver);

                return constructor.Invoke(parameters);
            };

        private object[] ResolveParameters(IEnumerable<ParameterInfo> parameters, IResolver resolver) =>
            parameters
                .Select(x => ResolveParameter(x, resolver))
                .ToArray();

        private object ResolveParameter(ParameterInfo parameter, IResolver resolver)
        {
            var isOptional = parameter.HasAttribute<InjectOptionalAttribute>();
            return resolver.ResolveInstance(parameter.ParameterType, isOptional);
        }

        private ConstructorInfo ResolveConstructor(Type type)
        {
            var constructor = type.GetConstructors().SingleOrDefault() ??
                              type.GetConstructors().FirstOrDefault(x => x.HasAttribute<InjectAttribute>());

            if (constructor == null)
                throw new Exception(
                    $"No constructor found with [Inject] attributes in multi-constructor type {type.Name}.");

            return constructor;
        }

        #endregion

        #region Inject

        public void Inject(object obj) =>
            Inject(obj, null);

        public void Inject(object obj, IResolver overrideResolver)
        {
            var resolver = this.WithOverride(overrideResolver);
            
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            
            if (obj is GameObject)
                InjectGameObject((GameObject) obj, resolver);
            else
                InjectObject(obj, resolver);
        }

        private void InjectObject(object obj, IResolver resolver)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            obj.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .ForEach(field => InjectField(obj, field, resolver));

            obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .ForEach(property => InjectProperty(obj, property, resolver));
        }

        private void InjectProperty(object obj, PropertyInfo property, IResolver resolver)
        {
            var inject = property.GetAttribute<InjectAttribute>();
            if (inject == null)
                return;
            
            var value = resolver.ResolveInstance(property.PropertyType, inject.Optional);
            _logger?.Log($"Injecting {obj.GetType().Name}.{property.Name} ({property.PropertyType.Name}) <= {FormatValue(value)}");
            property.SetValue(obj, value, null);
        }

        private void InjectField(object obj, FieldInfo field, IResolver resolver)
        {
            var inject = field.GetAttribute<InjectAttribute>();
            if (inject == null)
                return;
            
            var value = resolver.ResolveInstance(field.FieldType, inject.Optional);
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

        private bool IsValidComponent(MonoBehaviour behaviour)
        {
            if (behaviour != null)
                return true;
            
            _logger.LogWarning(nameof(Container), "Skipping null MonoBehaviour.");
            return false;
        }
        
        private void InjectGameObject(GameObject go, IResolver resolver)
        {
            go.GetComponents<MonoBehaviour>()
                .Where(IsValidComponent)
                .ForEach(x => InjectObject(x, resolver));
            
            go.Descendants()
                .ForEach(x => InjectGameObject(x, resolver));
        }

        private void InjectMethods(GameObject go, IResolver resolver)
        {
            go.GetComponents<MonoBehaviour>()
                .Where(IsValidComponent)
                .ForEach(component => component.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(method => method.HasAttribute<InjectAttribute>())
                    .ForEach(method => InjectMethod(component, method, resolver)));
            
            go.Descendants()
                .ForEach(x => InjectMethods(x, resolver));
        }

        public void InjectAllGameObjects()
        {
            RootGameObjects.ForEach(x => InjectGameObject(x, this));
            RootGameObjects.ForEach(x => InjectMethods(x, this));
        }

        private IEnumerable<GameObject> RootGameObjects =>
            AllScenes.SelectMany(x => x.GetRootGameObjects());

        private IEnumerable<Scene> AllScenes
        {
            get
            {
                for (var i = 0; i < SceneManager.sceneCount; i++)
                    yield return SceneManager.GetSceneAt(i);
            }
        }

        #endregion
    }
}