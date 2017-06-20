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
    public class Container
    {
        private enum Lifetime
        {
            Local,
            Single
        }
        
        private class Mapping
        {
            public Type AbstractType { get; }
            public Type ConcreteType { get; }
            public Lifetime Lifetime { get; }
            public object Singleton { get; set; }

            public Mapping(Type abstractType, Type concreteType, Lifetime lifetime)
            {
                AbstractType = abstractType;
                ConcreteType = concreteType;
                Lifetime = lifetime;
            }
        }
        
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();
        private readonly List<Mapping> _mappings = new List<Mapping>();
        private readonly ILogger _logger;

        public Container(ILogger logger = null)
        {
            _logger = logger;
        }

        public Container BindInstance<T>(T instance)
        {
            _instances[typeof(T)] = instance;
            return this;
        }

        public Container BindInstance(object instance)
        {
            _instances[instance.GetType()] = instance;
            return this;
        }

        public Container BindInstances(IEnumerable<object> instances)
        {
            instances.ForEach(x => BindInstance(x));
            return this;
        }

        public Container Bind<T>(Type type)
        {
            _mappings.Add(new Mapping(typeof(T), type, Lifetime.Local));
            return this;
        }

        public Container BindSelf<T>() =>
            Bind<T, T>();

        public Container BindSingle<T>(Type type)
        {
            _mappings.Add(new Mapping(typeof(T), type, Lifetime.Single));
            return this;
        }

        public Container Bind<T, U>() where U : T
        {
            Bind<T>(typeof(U));
            return this;
        }

        public Container BindSingle<T, U>() where U : T
        {
            BindSingle<T>(typeof(U));
            return this;
        }

        public object Resolve(Type abstractType, Container subContainer = null, bool isOptional = false)
        {
            var instance = ResolveInternal(abstractType, subContainer);
            if (instance == null && !isOptional)
                throw new Exception($"No mapping for required type {abstractType.Name}.");
        
            return instance;
        }

        private object ResolveInternal(Type abstractType, Container subContainer) =>
            ResolveSubContainer(abstractType, subContainer) ??
            ResolveInstance(abstractType) ??
            ResolveType(abstractType, subContainer) ??
            ResolveDefaultSelfBound(abstractType, subContainer);

        private static object ResolveSubContainer(Type abstractType, Container subContainer) =>
            subContainer?.Resolve(abstractType, null, true);

        private object ResolveInstance(Type abstractType) =>
            _instances.GetOptionalValue(abstractType);

        private object ResolveType(Type abstractType, Container subContainer) =>
            GetInstance(_mappings.FirstOrDefault(x => x.AbstractType == abstractType), subContainer);

        private object ResolveDefaultSelfBound(Type abstractType, Container subContainer) =>
            !abstractType.IsAbstract
                ? Instantiate(abstractType, subContainer)
                : null;

        private object GetInstance(Mapping mapping, Container subContainer = null)
        {
            if (mapping == null)
                return null;
            
            if (mapping.Lifetime == Lifetime.Local)
                return Instantiate(mapping.ConcreteType, subContainer);

            return mapping.Singleton ?? (mapping.Singleton = Instantiate(mapping.ConcreteType, subContainer));
        }

        private object Instantiate(Type type, Container subContainer = null)
        {
            var constructor = ResolveConstructor(type);
            var parameters = ResolveParameters(constructor.GetParameters(), subContainer);

            return constructor.Invoke(parameters);
        }

        private object[] ResolveParameters(IEnumerable<ParameterInfo> parameters, Container subContainer = null) =>
            parameters
                .Select(x => ResolveParameter(x, subContainer))
                .ToArray();

        private object ResolveParameter(ParameterInfo parameter, Container subContainer)
        {
            var isOptional = parameter.HasAttribute<InjectOptionalAttribute>();
            return Resolve(parameter.ParameterType, subContainer, isOptional);
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

        public T Resolve<T>() =>
            (T) Resolve(typeof(T));

        public void Inject(object obj) => Inject(obj, null);

        public void Inject(object obj, Container subContainer)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            
            if (obj is GameObject)
                InjectGameObject((GameObject) obj, subContainer);
            else
                InjectObject(obj, subContainer);
        }

        private void InjectObject(object obj, Container subContainer)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            obj.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .ForEach(field => InjectField(obj, field, subContainer));

            obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .ForEach(property => InjectProperty(obj, property, subContainer));
        }

        private void InjectProperty(object obj, PropertyInfo property, Container subContainer)
        {
            var inject = property.GetAttribute<InjectAttribute>();
            if (inject == null)
                return;
            
            var value = Resolve(property.PropertyType, subContainer, inject.Optional);
            _logger?.Log($"Injecting {obj.GetType().Name}.{property.Name} ({property.PropertyType.Name}) <= {FormatValue(value)}");
            property.SetValue(obj, value, null);
        }

        private void InjectField(object obj, FieldInfo field, Container subContainer)
        {
            var inject = field.GetAttribute<InjectAttribute>();
            if (inject == null)
                return;
            
            var value = Resolve(field.FieldType, subContainer, inject.Optional);
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
        
        private void InjectGameObject(GameObject go, Container subContainer)
        {
            go.GetComponents<MonoBehaviour>()
                .Where(IsValidComponent)
                .ForEach(x => InjectObject(x, subContainer));
            
            go.Descendants()
                .ForEach(x => InjectGameObject(x, subContainer));
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
    }
}