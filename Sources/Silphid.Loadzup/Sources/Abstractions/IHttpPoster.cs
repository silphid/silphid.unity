using System;
using UnityEngine;

namespace Silphid.Loadzup
{
    public interface IHttpPoster
    {
        IObservable<T> Post<T>(Uri uri, WWWForm form, Options options = null);
    }
}