using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup.Http
{
    public class HttpRequester : IRequester
    {
        private static readonly string[] MeaningfulHeaders =
        {
            KnownHttpHeaders.ContentType,
            KnownHttpHeaders.LastModified,
            KnownHttpHeaders.ETag,
            KnownHttpHeaders.Status
        };

        public IObservable<Response> Request(Uri uri, Options options = null) =>
            ObservableWWW
                .GetWWW(uri.AbsoluteUri, options?.RequestHeaders)
                .Catch<WWW, WWWErrorException>(ex => Observable.Throw<WWW>(new RequestException(ex)))
                .Select(www => new Response(www.bytes, GetMeaningfulHeaders(www.responseHeaders)));

        public IObservable<Response> Post(Uri uri, WWWForm form, Options options = null) =>
            ObservableWWW
                .PostWWW(uri.AbsoluteUri, form, options?.RequestHeaders ?? new Dictionary<string, string>())
                .Catch<WWW, WWWErrorException>(ex => Observable.Throw<WWW>(new RequestException(ex)))
                .Select(www => new Response(www.bytes, GetMeaningfulHeaders(www.responseHeaders)));

        private Dictionary<string, string> GetMeaningfulHeaders(IDictionary<string, string> allHeaders)
        {
            return MeaningfulHeaders
                .Select(x => new KeyValuePair<string, string>(x, allHeaders.GetOptionalValue(x)))
                .Where(x => x.Value != null)
                .ToDictionary(x => x.Key, x => x.Value);
        }
    }
}