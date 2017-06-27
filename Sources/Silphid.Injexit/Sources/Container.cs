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
    public class Container : IContainer
    {
        public static readonly IContainer Null = new NullContainer();
        
        #region Private fields

        private readonly List<Binding> _bindings = new List<Binding>();
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

        public IContainer CreateChild() =>
            new Container(_logger);

        #endregion
        
        #region IBinder members

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

            var binding = new Binding(this, abstractionType, concretionType);
            _bindings.Add(binding);

            return binding;
        }

        #endregion
        
        #region IResolver members

        public Func<IResolver, object> ResolveFactory(Type abstractionType, bool isOptional = false, bool isFallbackToSelfBinding = true) =>
            ResolveFromTypeMappings(abstractionType) ??
            ResolveFromListMappings(abstractionType) ??
            ResolveSelfBinding(abstractionType, isFallbackToSelfBinding) ??
            ThrowIfNotOptional(abstractionType, isOptional);

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

        private Func<IResolver, object> ResolveFromTypeMappings(Type abstractType) =>
            ResolveFactoryInternal(ResolveType(abstractType));

        private Binding ResolveType(Type abstractType) =>
            _bindings.FirstOrDefault(x => x.AbstractionType == abstractType);

        private Func<IResolver, object> ResolveSelfBinding(Type abstractionType, bool isSelfBindingAllowed) =>
            isSelfBindingAllowed && !abstractionType.IsAbstract
                ? ResolveFactoryInternal(abstractionType)
                : NullFactory;

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
                ? resolver => factory(resolver.With(overrideResolver))
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

        #region IInjector members

        public void Inject(object obj, IResolver overrideResolver = null)
        {
            var resolver = this.With(overrideResolver);
            
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            
            if (obj is GameObject)
            {
                InjectGameObjectValues((GameObject) obj, resolver);
                InjectGameObjectMethods((GameObject) obj, resolver);
            }
            else
            {
                InjectObject(obj, resolver);
                InjectObjectMethods(obj, resolver);
            }
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
        
        private void InjectGameObjectValues(GameObject go, IResolver resolver)
        {
            go.GetComponents<MonoBehaviour>()
                .Where(IsValidComponent)
                .ForEach(x => InjectObject(x, resolver));
            
            go.Descendants()
                .ForEach(x => InjectGameObjectValues(x, resolver));
        }

        private void InjectGameObjectMethods(GameObject go, IResolver resolver)
        {
            go.GetComponents<MonoBehaviour>()
                .Where(IsValidComponent)
                .ForEach(component => InjectObjectMethods(component, resolver));
            
            go.Descendants()
                .ForEach(x => InjectGameObjectMethods(x, resolver));
        }

        private void InjectObjectMethods(object obj, IResolver resolver)
        {
            obj.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(method => method.HasAttribute<InjectAttribute>())
                .ForEach(method => InjectMethod(obj, method, resolver));
        }

        public void InjectAllGameObjects()
        {
            RootGameObjects.ForEach(x => InjectGameObjectValues(x, this));
            RootGameObjects.ForEach(x => InjectGameObjectMethods(x, this));
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