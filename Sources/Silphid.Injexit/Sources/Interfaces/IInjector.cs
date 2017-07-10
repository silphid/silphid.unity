using System.Collections.Generic;
using UnityEngine;

namespace Silphid.Injexit
{
    public interface IInjector
    {
        void Inject(object obj, IResolver overrideResolver = null);
        void InjectGameObjects(IEnumerable<GameObject> gameObjects);
    }
}