using System;
using System.Collections.Generic;
using log4net;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup.Http
{
    public class HttpRequester : IHttpRequester
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpRequester));

        private static readonly string NewLine = Environment.NewLine;

        public IObservable<Response> Request(Uri uri, Options options = null) =>
            ObservableWebRequest
                .Get(uri.AbsoluteUri, options?.Headers)
                .DoOnSubscribe(() =>
                    Log.Info($"GET {uri}{NewLine}Headers: {options?.Headers}"))
                .DoOnError(ex =>
                    Log.Error($"Failed GET {uri}{NewLine} Headers: {options?.Headers}", ex))
                .Do(www =>
                {
                    if (www.GetResponseHeaders() == null)
                        Log.Warn($"There is no header in the response {uri}");
                })
                .Select(www => new Response(www.responseCode, www.downloadHandler.data,
                    www.GetResponseHeaders() ?? new Dictionary<string, string>(), options));

        public IObservable<Response> Get(Uri uri, Options options = null) =>
            Request(uri, options);

        public IObservable<Response> Post(Uri uri, WWWForm form, Options options = null) =>
            ObservableWebRequest
                .Post(uri.AbsoluteUri, form, options?.Headers)
                .DoOnSubscribe(() =>
                    Log.Info($"POST {uri}{NewLine}Form: {form}{NewLine}Headers: {options?.Headers}"))
                .DoOnError(ex =>
                    Log.Error($"Failed POST {uri}{NewLine}Form: {form}{NewLine}Headers: {options?.Headers}", ex))
                .Do(www =>
                {
                    if (www.GetResponseHeaders() == null)
                        Log.Warn($"There is no header in the response {uri}");
                })
                .Select(www => new Response(www.responseCode, www.downloadHandler.data,
                    www.GetResponseHeaders() ?? new Dictionary<string, string>(), options));


        public IObservable<Response> Put(Uri uri, string body, Options options = null) =>
            ObservableWebRequest
                .Put(uri.AbsoluteUri, body, options?.Headers)
                .DoOnSubscribe(() =>
                    Log.Info($"PUT {uri}{NewLine}Body: {body}{NewLine}Headers: {options?.Headers}"))
                .DoOnError(ex =>
                    Log.Error($"Failed PUT {uri}{NewLine}Body: {body}{NewLine}Headers: {options?.Headers}", ex))
                .Do(www =>
                {
                    if (www.GetResponseHeaders() == null)
                        Log.Warn($"There is no header in the response {uri}");
                })
                .Select(www => new Response(www.responseCode, www.downloadHandler.data,
                    www.GetResponseHeaders() ?? new Dictionary<string, string>(), options))
    }
}