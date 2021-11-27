using System;
using System.Threading;
using JetBrains.Annotations;
using Silphid.Injexit;
using Silphid.Showzup.Recipes;
using UnityEngine;

namespace Silphid.Showzup
{
    public interface IViewLoader
    {
        [Pure]
        IObservable<IView> Load(Transform parent, Recipe recipe, IContainer container, CancellationToken cancellationToken);
    }
}