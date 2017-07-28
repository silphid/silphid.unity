using System;
using UniRx;

namespace Silphid.Loadzup
{
    public interface ILoader
    {
        bool Supports<T>(Uri uri);
        IObservable<T> Load<T>(Uri uri, Options options = null);
    }
}