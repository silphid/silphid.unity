using System;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;

namespace Silphid.Showzup
{
    public interface IViewLoader
    {
        [Pure] IObservable<IView> Load(Transform parent, ViewInfo viewInfo, CancellationToken cancellationToken);
    }
}