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

        private readonly ILogger _logger;

        public HttpRequester(ILogger logger = null)
        {
            _logger = logger;
        }

        public IObservable<Response> Request(Uri uri, Options options = null) =>
            ObservableWWW
                .GetWWW(uri.AbsoluteUri, options?.RequestHeaders)
                .DoOnSubscribe(() => Log($"GET {uri}"))
                .DoOnError(LogError)
                .Catch<WWW, WWWErrorException>(ex => Observable.Throw<WWW>(new RequestException(ex)))
                .Select(www => new Response(www.bytes, GetMeaningfulHeaders(www.responseHeaders)));

        public IObservable<Response> Post(Uri uri, WWWForm form, Options options = null)
        {
            var headers = options?.RequestHeaders ?? new Dictionary<string, string>();
            return ObservableWWW
                .PostWWW(uri.AbsoluteUri, form, headers)
                .DoOnSubscribe(() => Log($"POST {uri}\r\nForm: {form}\r\nHeaders: {headers}"))
                .DoOnError(LogError)
                .Catch<WWW, WWWErrorException>(ex => Observable.Throw<WWW>(new RequestException(ex)))
                .Select(www => new Response(www.bytes, GetMeaningfulHeaders(www.responseHeaders)));
        }

        private void Log(string message) =>
            _logger?.Log(nameof(HttpRequester), message);

        private void LogError(Exception exception) =>
            _logger?.LogError(nameof(HttpRequester), $"Request failed: {exception}");

        private Dictionary<string, string> GetMeaningfulHeaders(IDictionary<string, string> allHeaders)
        {
            return MeaningfulHeaders
                .Select(x => new KeyValuePair<string, string>(x, allHeaders.GetValueOrDefault(x)))
                .Where(x => x.Value != null)
                .ToDictionary(x => x.Key, x => x.Value);
        }
    }
}