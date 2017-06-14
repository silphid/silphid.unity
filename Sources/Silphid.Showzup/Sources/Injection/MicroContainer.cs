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
    public class MicroContainer
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

        public MicroContainer(ILogger logger = null)
        {
            _logger = logger;
        }

        public void BindInstance<T>(T instance)
        {
            _instances[typeof(T)] = instance;
        }

        public void Bind<T>(Type type)
        {
            _mappings.Add(new Mapping(typeof(T), type, Lifetime.Local));
        }

        public void BindSingle<T>(Type type)
        {
            _mappings.Add(new Mapping(typeof(T), type, Lifetime.Single));
        }

        public void Bind<T, U>() where U : T
        {
            Bind<T>(typeof(U));
        }

        public void BindSingle<T, U>() where U : T
        {
            BindSingle<T>(typeof(U));
        }

        public object Resolve(Type abstractType, Dictionary<Type, object> extraBindings = null, bool isOptional = false)
        {
            _logger?.Log($"Resolving type {abstractType.Name}...");
            
            var instance = ResolveInternal(abstractType, extraBindings);
            if (instance == null && !isOptional)
                throw new Exception($"No mapping for required type {abstractType.Name}.");
        
            return instance;
        }

        private object ResolveInternal(Type abstractType, Dictionary<Type, object> extraBindings) =>
            ResolveExtraBinding(abstractType, extraBindings) ??
            ResolveInstance(abstractType) ??
            ResolveType(abstractType, extraBindings) ??
            ResolveDefaultSelfBound(abstractType, extraBindings);

        private static object ResolveExtraBinding(Type abstractType, Dictionary<Type, object> extraBindings) =>
            extraBindings?.GetOptionalValue(abstractType);

        private object ResolveInstance(Type abstractType) =>
            _instances.GetOptionalValue(abstractType);

        private object ResolveType(Type abstractType, Dictionary<Type, object> extraBindings) =>
            GetInstance(_mappings.FirstOrDefault(x => x.AbstractType == abstractType), extraBindings);

        private object ResolveDefaultSelfBound(Type abstractType, Dictionary<Type, object> extraBindings) =>
            !abstractType.IsAbstract
                ? Instantiate(abstractType, extraBindings)
                : null;

        private object GetInstance(Mapping mapping, Dictionary<Type, object> extraBindings = null)
        {
            if (mapping == null)
                return null;
            
            if (mapping.Lifetime == Lifetime.Local)
                return Instantiate(mapping.ConcreteType, extraBindings);

            return mapping.Singleton ?? (mapping.Singleton = Instantiate(mapping.ConcreteType, extraBindings));
        }

        private object Instantiate(Type type, Dictionary<Type, object> extraBindings = null)
        {
            var constructor = ResolveConstructor(type);
            var parameters = ResolveParameters(constructor.GetParameters(), extraBindings);

            return constructor.Invoke(parameters);
        }

        private object[] ResolveParameters(ParameterInfo[] parameters, Dictionary<Type, object> extraBindings = null) =>
            parameters
                .Select(x => ResolveParameter(x, extraBindings))
                .ToArray();

        private object ResolveParameter(ParameterInfo parameter, Dictionary<Type, object> extraBindings)
        {
            var isOptional = parameter.HasAttribute<InjectOptionalAttribute>();
            return Resolve(parameter.ParameterType, extraBindings, isOptional);
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

        public void Inject(object obj, Dictionary<Type, object> extraBindings)
        {
            obj.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .ForEach(field => InjectField(obj, field, extraBindings));

            obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .ForEach(property => InjectProperty(obj, property, extraBindings));
        }

        private void InjectProperty(object obj, PropertyInfo property, Dictionary<Type, object> extraBindings)
        {
            var inject = property.GetAttribute<InjectAttribute>();
            if (inject == null)
                return;
            
            var value = Resolve(property.PropertyType, extraBindings, inject.Optional);
            _logger?.Log($"Injecting property {obj.GetType().Name}.{property.Name}: {value}");
            property.SetValue(obj, value, null);
        }

        private void InjectField(object obj, FieldInfo field, Dictionary<Type, object> extraBindings)
        {
            var inject = field.GetAttribute<InjectAttribute>();
            if (inject == null)
                return;
            
            var value = Resolve(field.FieldType, extraBindings, inject.Optional);
            _logger?.Log($"Injecting field {obj.GetType().Name}.{field.Name}: {value}");
            field.SetValue(obj, value);
        }

        private void InjectMethod(object obj, MethodInfo method)
        {
            var inject = method.GetAttribute<InjectAttribute>();
            if (inject == null)
                return;
            
            var parameters = ResolveParameters(method.GetParameters());
            _logger?.Log($"Injecting method {obj.GetType().Name}.{method.Name}: {parameters.ToDelimitedString(", ")}");
            method.Invoke(obj, parameters);
        }

        public void Inject(GameObject go) =>
            Inject(go, null);

        public void Inject(GameObject go, Dictionary<Type, object> extraBindings)
        {
            go.GetComponents<MonoBehaviour>()
                .ForEach(x => Inject(x, extraBindings));
        }

        private void CallInjectMethods(GameObject go)
        {
            go.GetComponents<MonoBehaviour>()
                .ForEach(component => component.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(method => method.HasAttribute<InjectAttribute>())
                    .ForEach(method => InjectMethod(component, method)));
        }

        public void InjectAllGameObjects()
        {
            AllGameObjects.ForEach(Inject);
            AllGameObjects.ForEach(CallInjectMethods);
        }

        private IEnumerable<GameObject> AllGameObjects =>
            AllScenes
                .SelectMany(x => x.GetRootGameObjects().SelectMany(y => y.SelfAndDescendants()));

        private IEnumerable<Scene> AllScenes
        {
            get
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                    yield return SceneManager.GetSceneAt(i);
            }
        }

        public Dictionary<Type, object> SubBind<T>(T value) =>
            new Dictionary<Type, object>
            {
                [typeof(T)] = value
            };

        public Dictionary<Type, object> SubBind(object value) =>
            new Dictionary<Type, object>
            {
                [value.GetType()] = value
            };
    }
}