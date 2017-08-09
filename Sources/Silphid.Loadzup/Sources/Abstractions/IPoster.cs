using System;
using UnityEngine;

namespace Silphid.Loadzup
{
    public interface IPoster
    {
        IObservable<T> Post<T>(Uri uri, WWWForm form, Options options = null);
    }
}