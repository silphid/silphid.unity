using System;

namespace Silphid.Loadzup
{
    public interface IRequester
    {
        IObservable<Response> Request(Uri uri, Options options = null);
    }
}