using System;
using UnityEngine;

namespace Silphid.Loadzup
{
    public interface IPutter
    {
        IObservable<T> Put<T>(Uri uri, string body, Options options = null);
    }
}