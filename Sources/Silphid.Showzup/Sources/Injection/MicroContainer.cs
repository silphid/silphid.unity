using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silphid.Extensions;
using Zenject;

namespace Silphid.Showzup.Injection
{
    public class MicroContainer
    {
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Type> _types = new Dictionary<Type, Type>();

        public void Bind<T>(T instance)
        {
            _instances[typeof(T)] = instance;
        }

        public void Bind<T>(Type type)
        {
            _types[typeof(T)] = type;
        }

        public void Bind<T, U>()
        {
            Bind<T>(typeof(U));
        }

        public object Resolve(Type interfaceType, Dictionary<Type, object> extraBindings = null)
        {
            var instance = ResolveInternal(interfaceType, extraBindings);
            if (instance != null)
                return instance;

            throw new Exception($"No type or instance registered for type {interfaceType.Name}.");
        }

        private object ResolveInternal(Type interfaceType, Dictionary<Type, object> extraBindings) =>
            ResolveExtraBinding(interfaceType, extraBindings) ??
            ResolveInstance(interfaceType) ??
            ResolveType(interfaceType, extraBindings) ??
            ResolveSelfBoundConcreteType(interfaceType, extraBindings);

        private static object ResolveExtraBinding(Type interfaceType, Dictionary<Type, object> extraBindings) =>
            extraBindings.GetOptionalValue(interfaceType);

        private object ResolveInstance(Type interfaceType) =>
            _instances.GetOptionalValue(interfaceType);

        private object ResolveType(Type interfaceType, Dictionary<Type, object> extraBindings)
        {
            var type = _types.GetOptionalValue(interfaceType);
            return type != null
                ? Instantiate(type, extraBindings)
                : null;
        }

        private object ResolveSelfBoundConcreteType(Type interfaceType, Dictionary<Type, object> extraBindings) =>
            !interfaceType.IsAbstract
                ? Instantiate(interfaceType, extraBindings)
                : null;

        private object Instantiate(Type type, Dictionary<Type, object> extraBindings)
        {
            var constructor = ResolveConstructor(type);
            var parameters = ResolveParameters(constructor, extraBindings);

            return constructor.Invoke(parameters);
        }

        private object[] ResolveParameters(ConstructorInfo constructor, Dictionary<Type, object> extraBindings) =>
            constructor.GetParameters()
                .Select(x => ResolveInternal(x.ParameterType, extraBindings))
                .ToArray();

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

        public void Inject(object obj, Dictionary<Type, object> extraBindings)
        {
            obj.GetType()
                .GetFields(BindingFlags.Instance)
                .Where(x => x.HasAttribute<InjectAttribute>())
                .ForEach(x => x.SetValue(obj, Resolve(x.FieldType, extraBindings)));

            obj.GetType()
                .GetProperties(BindingFlags.Instance)
                .Where(x => x.HasAttribute<InjectAttribute>())
                .ForEach(x => x.SetValue(obj, Resolve(x.PropertyType, extraBindings), null));
        }
    }
}