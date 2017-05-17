#if JSON_NET && ZENJECT

using System;
using Newtonsoft.Json.Serialization;
using Zenject;

namespace Silphid.Loadzup.JsonNet
{
    public class ZenjectContractResolver : DefaultContractResolver
    {
        private readonly DiContainer _container;

        public ZenjectContractResolver(DiContainer container)
        {
            _container = container;
        }

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var contract = base.CreateObjectContract(objectType);

            if (_container.HasBinding(new InjectContext(_container, objectType)))
                contract.DefaultCreator = () => _container.Resolve(objectType);

            return contract;
        }
    }
}

#endif