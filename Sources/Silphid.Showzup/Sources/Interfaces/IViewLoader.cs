using System;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public interface IViewLoader
    {
        [Pure] IObservable<IView> Load(Transform parent, ViewInfo viewInfo, CancellationToken cancellationToken);
    }
}