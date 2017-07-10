using System;
using System.Collections.Generic;
using UnityEngine;

namespace Silphid.Injexit
{
    internal class NullContainer : IContainer
    {
        public IContainer CreateChild() => Container.Null;

        public IBinding Bind(Type abstractionType, Type concretionType) => Binding.Null;
        public IBinding BindInstance(Type abstractionType, object instance) => Binding.Null;
        public void BindForward(Type sourceAbstractionType, Type targetAbstractionType) {}

        public Func<IResolver, object> ResolveFactory(Type abstractionType, string id = null, bool isOptional = false) => _ => null;
        
        public void Inject(object obj, IResolver overrideResolver = null) {}
        public void InjectGameObjects(IEnumerable<GameObject> gameObjects) {}

        public void Dispose() {}
    }
}