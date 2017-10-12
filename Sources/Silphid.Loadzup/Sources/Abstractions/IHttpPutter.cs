using System;

namespace Silphid.Loadzup
{
    public interface IHttpPutter
    {
        IObservable<T> Put<T>(Uri uri, string body, Options options = null);
    }
}