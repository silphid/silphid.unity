using System;

namespace Silphid.Loadzup
{
    public interface IHttpRequester
    {
        IObservable<Response> Request(Uri uri, Options options = null);
    }
}