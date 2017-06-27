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
    public class Container : IResolver
    {
        #region Lifetime enum

        private enum Lifetime
        {
            Transient,
            Single
        }
        
        #endregion

        #region Mapping inner class

        private class Mapping
        {
            public Type AbstractionType { get; }
            public Type ConcretionType { get; }
            public Lifetime Lifetime { get; }
            public IResolver SubResolver { get; }
            public object Singleton { get; set; }

            public Mapping(Type abstractionType, Type concretionType, Lifetime lifetime, IResolver subResolver)
            {
                AbstractionType = abstractionType;
                ConcretionType = concretionType;
                Lifetime = lifetime;
                SubResolver = subResolver;
            }
        }

        #endregion
        
        #region Private fields

        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();
        private readonly List<Mapping> _mappings = new List<Mapping>();
        private readonly List<Mapping> _listMappings = new List<Mapping>();
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public Container(ILogger logger = null)
        {
            _logger = logger;
        }

        #endregion
        
        #region BindInstance(s)

        public Container BindInstance(Type abstractionType, object instance)
        {
            if (!instance.GetType().IsAssignableTo(abstractionType))
                throw new InvalidOperationException($"Instance type {instance.GetType().Name} must be assignable to abstraction type {abstractionType.Name}.");
                
            _instances[abstractionType] = instance;
            
            abstractionType.GetAttributes<BindAttribute>()
                .Select(x => x.Type)
                .ForEach(x => BindInstance(x, instance));
            
            return this;
        }

        public Container BindInstance<T>(T instance) =>
            BindInstance(typeof(T), instance);

        public Container BindInstance(object instance) =>
            BindInstance(instance.GetType(), instance);

        public Container BindInstances(IEnumerable<object> instances)
        {
            instances.ForEach(x => BindInstance(x));
            return this;
        }

        public Container BindInstances(params object[] instances) =>
            BindInstances((IEnumerable<object>) instances);

        #endregion

        #region BindInstance(s)AsList

        public Container BindInstanceAsList(Type abstractionType, object instance)
        {
            if (!instance.GetType().IsAssignableTo(abstractionType))
                throw new InvalidOperationException($"Instance type {instance.GetType().Name} must be assignable to abstraction type {abstractionType.Name}.");
                
            _instances[abstractionType] = instance;
            
            abstractionType.GetAttributes<BindAttribute>()
                .Select(x => x.Type)
                .ForEach(x => BindInstanceAsList(x, instance));
            
            return this;
        }

        public Container BindInstanceAsList<T>(T instance) =>
            BindInstanceAsList(typeof(T), instance);

        public Container BindInstanceAsList(object instance) =>
            BindInstanceAsList(instance.GetType(), instance);

        public Container BindInstancesAsList(IEnumerable<object> instances)
        {
            instances.ForEach(x => BindInstanceAsList(x));
            return this;
        }

        public Container BindInstancesAsList(params object[] instances) =>
            BindInstancesAsList((IEnumerable<object>) instances);

        #endregion

        #region Bind

        public Container Bind(Type abstractionType, Type concretionType, IResolver subResolver = null)
        {
            if (!concretionType.IsAssignableTo(abstractionType))
                throw new InvalidOperationException($"Concretion type {concretionType.Name} must be assignable to abstraction type {abstractionType.Name}.");

            _mappings.Add(new Mapping(abstractionType, concretionType, Lifetime.Transient, subResolver));           
            return this;
        }

        public Container Bind<TAbstraction>(Type concretionType, IResolver subResolver = null) =>
            Bind(typeof(TAbstraction), concretionType, subResolver);

        public Container Bind<TAbstraction, TConcretion>(Container subResolver = null) where TConcretion : TAbstraction =>
            Bind(typeof(TAbstraction), typeof(TConcretion), subResolver);

        #endregion

        #region BindAsList

        public Container BindAsList(Type abstractionType, Type concretionType, IResolver subResolver = null)
        {
            if (!concretionType.IsAssignableTo(abstractionType))
                throw new InvalidOperationException($"Concretion type {concretionType.Name} must be assignable to abstraction type {abstractionType.Name}.");

            _listMappings.Add(new Mapping(abstractionType, concretionType, Lifetime.Transient, subResolver));           
            return this;
        }

        public Container BindAsList<TAbstraction>(Type concretionType, IResolver subResolver = null) =>
            BindAsList(typeof(TAbstraction), concretionType, subResolver);

        public Container BindAsList<TAbstraction, TConcretion>(Container subResolver = null) where TConcretion : TAbstraction =>
            BindAsList(typeof(TAbstraction), typeof(TConcretion), subResolver);

        #endregion

        #region BindSelf

        public Container BindSelf<T>(Container subResolver = null) =>
            Bind<T, T>(subResolver);

        #endregion

        #region BindSingle

        public Container BindSingle(Type abstractionType, Type targetType, IResolver subResolver = null)
        {
            _mappings.Add(new Mapping(abstractionType, targetType, Lifetime.Single, subResolver));
            return this;
        }

        public Container BindSingle<TAbstraction>(Type concretionType, IResolver subResolver = null) =>
            BindSingle(typeof(TAbstraction), concretionType, subResolver);

        public Container BindSingle<TAbstraction, TConcretion>(IResolver subResolver = null) where TConcretion : TAbstraction =>
            BindSingle(typeof(TAbstraction), typeof(TConcretion), subResolver);

        #endregion

        #region BindSingleAsList

        public Container BindSingleAsList(Type abstractionType, Type targetType, IResolver subResolver = null)
        {
            _listMappings.Add(new Mapping(abstractionType, targetType, Lifetime.Single, subResolver));
            return this;
        }

        public Container BindSingleAsList<TAbstraction>(Type concretionType, IResolver subResolver = null) =>
            BindSingleAsList(typeof(TAbstraction), concretionType, subResolver);

        public Container BindSingleAsList<TAbstraction, TConcretion>(IResolver subResolver = null) where TConcretion : TAbstraction =>
            BindSingleAsList(typeof(TAbstraction), typeof(TConcretion), subResolver);

        #endregion
        
        #region Resolve

        public object Resolve(Type abstractionType, IResolver subResolver = null, bool isOptional = false)
        {
            var instance = ResolveInternal(abstractionType, subResolver);
            if (instance == null && !isOptional)
                throw new Exception($"No mapping for required type {abstractionType.Name}.");
        
            return instance;
        }

        private object ResolveInternal(Type abstractionType, IResolver subResolver = null) =>
            ResolveAsList(abstractionType, subResolver) ??
            ResolveSubContainer(abstractionType, subResolver) ??
            ResolveInstance(abstractionType) ??
            ResolveType(abstractionType, subResolver) ??
            ResolveDefaultSelfBound(abstractionType, subResolver);

        private static object ResolveAsList(Type abstractionType, IResolver subResolver)
        {
        }

        private static object ResolveSubContainer(Type abstractionType, IResolver subResolver) =>
            subResolver?.Resolve(abstractionType, null, true);

        private object ResolveInstance(Type abstractType) =>
            _instances.GetOptionalValue(abstractType);

        private object ResolveType(Type abstractType, IResolver subResolver) =>
            GetInstance(_mappings.FirstOrDefault(x => x.AbstractionType == abstractType), subResolver);

        private object ResolveDefaultSelfBound(Type abstractionType, IResolver subResolver) =>
            !abstractionType.IsAbstract
                ? Instantiate(abstractionType, subResolver)
                : null;

        private object GetInstance(Mapping mapping, IResolver subResolver = null)
        {
            if (mapping == null)
                return null;
            
            if (mapping.Lifetime == Lifetime.Transient)
                return Instantiate(mapping, subResolver);

            return mapping.Singleton ?? (mapping.Singleton = Instantiate(mapping.ConcretionType, subResolver));
        }

        private object Instantiate(Mapping mapping, IResolver subResolver = null) =>
            Instantiate(
                mapping.ConcretionType,
                new CompositeResolver(
                    mapping.SubResolver,
                    subResolver,
                    this));

        private object Instantiate(Type concretionType, IResolver subResolver)
        {
            var constructor = ResolveConstructor(concretionType);
            var parameters = ResolveParameters(constructor.GetParameters(), subResolver);

            return constructor.Invoke(parameters);
        }

        private object[] ResolveParameters(IEnumerable<ParameterInfo> parameters, IResolver subResolver = null) =>
            parameters
                .Select(x => ResolveParameter(x, subResolver))
                .ToArray();

        private object ResolveParameter(ParameterInfo parameter, IResolver subResolver)
        {
            var isOptional = parameter.HasAttribute<InjectOptionalAttribute>();
            return Resolve(parameter.ParameterType, subResolver, isOptional);
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

        public T Resolve<T>(IResolver subResolver = null) =>
            (T) Resolve(typeof(T), subResolver);

        #endregion

        #region Inject

        public void Inject(object obj) => Inject(obj, null);

        public void Inject(object obj, IResolver subResolver)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            
            if (obj is GameObject)
                InjectGameObject((GameObject) obj, subResolver);
            else
                InjectObject(obj, subResolver);
        }

        private void InjectObject(object obj, IResolver subResolver)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            obj.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .ForEach(field => InjectField(obj, field, subResolver));

            obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .ForEach(property => InjectProperty(obj, property, subResolver));
        }

        private void InjectProperty(object obj, PropertyInfo property, IResolver subResolver)
        {
            var inject = property.GetAttribute<InjectAttribute>();
            if (inject == null)
                return;
            
            var value = Resolve(property.PropertyType, subResolver, inject.Optional);
            _logger?.Log($"Injecting {obj.GetType().Name}.{property.Name} ({property.PropertyType.Name}) <= {FormatValue(value)}");
            property.SetValue(obj, value, null);
        }

        private void InjectField(object obj, FieldInfo field, IResolver subResolver)
        {
            var inject = field.GetAttribute<InjectAttribute>();
            if (inject == null)
                return;
            
            var value = Resolve(field.FieldType, subResolver, inject.Optional);
            _logger?.Log($"Injecting {obj.GetType().Name}.{field.Name} ({field.FieldType.Name}) <= {FormatValue(value)}");
            field.SetValue(obj, value);
        }

        private void InjectMethod(object obj, MethodInfo method)
        {
            var inject = method.GetAttribute<InjectAttribute>();
            if (inject == null)
                return;
            
            var parameters = ResolveParameters(method.GetParameters());
            _logger?.Log($"Injecting {obj.GetType().Name}.{method.Name}({FormatParameters(parameters)})");
            method.Invoke(obj, parameters);
        }

        private static string FormatParameters(object[] parameters) =>
            parameters.Select(FormatValue).ToDelimitedString(", ");

        private static string FormatValue(object value) =>
            value?.ToString() ?? "null";

        private void InjectGameObject(GameObject go) =>
            InjectGameObject(go, null);

        private bool IsValidComponent(MonoBehaviour behaviour)
        {
            if (behaviour != null)
                return true;
            
            _logger.LogWarning(nameof(Container), "Skipping null MonoBehaviour.");
            return false;
        }
        
        private void InjectGameObject(GameObject go, IResolver subResolver)
        {
            go.GetComponents<MonoBehaviour>()
                .Where(IsValidComponent)
                .ForEach(x => InjectObject(x, subResolver));
            
            go.Descendants()
                .ForEach(x => InjectGameObject(x, subResolver));
        }

        private void InjectMethods(GameObject go)
        {
            go.GetComponents<MonoBehaviour>()
                .Where(IsValidComponent)
                .ForEach(component => component.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(method => method.HasAttribute<InjectAttribute>())
                    .ForEach(method => InjectMethod(component, method)));
            
            go.Descendants()
                .ForEach(InjectMethods);
        }

        public void InjectAllGameObjects()
        {
            RootGameObjects.ForEach(InjectGameObject);
            RootGameObjects.ForEach(InjectMethods);
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