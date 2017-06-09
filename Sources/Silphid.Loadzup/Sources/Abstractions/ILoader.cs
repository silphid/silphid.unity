using System;

namespace Silphid.Loadzup
{
    public interface ILoader
    {
        bool Supports(Uri uri);
        UniRx.IObservable<T> Load<T>(Uri uri, Options options = null);
    }
}