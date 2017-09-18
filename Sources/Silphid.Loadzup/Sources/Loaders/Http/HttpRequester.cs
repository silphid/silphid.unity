using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
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

        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpRequester));

        public IObservable<Response> Request(Uri uri, Options options = null) =>
            ObservableWWW
                .GetWWW(uri.AbsoluteUri, options?.RequestHeaders)
                .DoOnSubscribe(() => LogMessage($"GET {uri}"))
                .DoOnError(LogError)
                .Catch<WWW, WWWErrorException>(ex => Observable.Throw<WWW>(new RequestException(ex)))
                .Select(www => new Response(www.bytes, GetMeaningfulHeaders(www.responseHeaders)));

        public IObservable<Response> Post(Uri uri, WWWForm form, Options options = null)
        {
            var headers = options?.RequestHeaders ?? new Dictionary<string, string>();
            return ObservableWWW
                .PostWWW(uri.AbsoluteUri, form, headers)
                .DoOnSubscribe(() => LogMessage($"POST {uri}\r\nForm: {form}\r\nHeaders: {headers}"))
                .DoOnError(LogError)
                .Catch<WWW, WWWErrorException>(ex => Observable.Throw<WWW>(new RequestException(ex)))
                .Select(www => new Response(www.bytes, GetMeaningfulHeaders(www.responseHeaders)));
        }

        private void LogMessage(string message) =>
            Log.Debug(message);

        private void LogError(Exception exception) =>
            Log.Error($"Request failed: {exception}");

        private Dictionary<string, string> GetMeaningfulHeaders(IDictionary<string, string> allHeaders)
        {
            return MeaningfulHeaders
                .Select(x => new KeyValuePair<string, string>(x, allHeaders.GetValueOrDefault(x)))
                .Where(x => x.Value != null)
                .ToDictionary(x => x.Key, x => x.Value);
        }
    }
}