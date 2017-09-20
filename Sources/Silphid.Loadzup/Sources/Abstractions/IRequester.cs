using System;
using UnityEngine;

namespace Silphid.Loadzup
{
    public interface IRequester
    {
        IObservable<Response> Request(Uri uri, Options options = null);
        IObservable<Response> Get(Uri uri, Options options = null);
        IObservable<Response> Post(Uri uri, WWWForm form, Options options = null);
        IObservable<Response> Put(Uri uri, string body, Options options = null);
    }
}