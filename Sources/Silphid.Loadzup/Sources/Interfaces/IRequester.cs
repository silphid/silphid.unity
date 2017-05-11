using System;

namespace Silphid.Loadzup
{
    public interface IRequester
    {
        UniRx.IObservable<Response> Request(Uri uri, Options options = null);
    }
}