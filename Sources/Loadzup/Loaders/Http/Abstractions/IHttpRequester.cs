using System;

namespace Silphid.Loadzup.Http
{
    public interface IHttpRequester
    {
        IObservable<Response> Request(Uri uri, IOptions options = null);
    }
}