using System;

namespace Silphid.Loadzup
{
    public interface ILoader
    {
        bool Supports<T>(Uri uri);
        IObservable<T> Load<T>(Uri uri, IOptions options = null);
    }
}