using System.Linq;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Injexit
{
    public static class GameObjectExtensions
    {
        public static IContainer GetParentContainer(this GameObject This) =>
            This.Ancestors<IContainerProvider>()
                .First()
                .Container;

        public static IContainer GetParentContainer(this Component This) =>
            This.gameObject.GetParentContainer();
    }
}