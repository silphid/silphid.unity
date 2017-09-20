using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Silphid.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

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
            ObservableWebRequest
                .Get(uri.AbsoluteUri, options?.RequestHeaders)
                .DoOnSubscribe(() => LogMessage($"GET {uri}"))
                .DoOnError(LogError)
                .Select(www => new Response(www.downloadHandler.data, GetMeaningfulHeaders(www.GetResponseHeaders())));

        public IObservable<Response> Get(Uri uri, Options options = null) => Request(uri, options);

        public IObservable<Response> Post(Uri uri, WWWForm form, Options options = null) => ObservableWebRequest
            .Post(uri.AbsoluteUri, form, options?.RequestHeaders)
            .DoOnSubscribe(() => LogMessage($"POST {uri}\r\nForm: {form}\r\nHeaders: {options?.RequestHeaders}"))
            .DoOnError(LogError)
            .Select(www => new Response(www.downloadHandler.data, GetMeaningfulHeaders(www.GetResponseHeaders())));

        public IObservable<Response> Put(Uri uri, string body, Options options = null) => ObservableWebRequest
            .Put(uri.AbsoluteUri, body, options?.RequestHeaders)
            .DoOnSubscribe(() => LogMessage($"PUT {uri}\r\nBody: {body}\r\nHeaders: {options?.RequestHeaders}"))
            .DoOnError(LogError)
            .Select(www => new Response(www.downloadHandler.data, GetMeaningfulHeaders(www.GetResponseHeaders())));

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