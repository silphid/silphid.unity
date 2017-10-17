using System;
using System.Collections.Generic;
using log4net;
using UniRx;
using UnityEngine.Networking;

namespace Silphid.Loadzup.Http
{
    public class HttpRequester : IHttpRequester
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpRequester));

        private static readonly string NewLine = Environment.NewLine;

        public IObservable<Response> Request(Uri uri, Options options = null) =>
            RequestInternal(uri.AbsoluteUri, options)
                .DoOnSubscribe(() =>
                    Log.Info(GetLogMessage(uri, options)))
                .DoOnError(ex =>
                    Log.Error($"Failed {GetLogMessage(uri, options)}", ex))
                .Select(www => new
                {
                    WWW = www,
                    Headers = www.GetResponseHeaders()
                })
                .Do(x =>
                {
                    if (uri.Scheme != Scheme.StreamingFile && x.Headers == null)
                        Log.Warn($"No headers in response from {uri}");
                })
                .Select(x => new Response(x.WWW.responseCode, x.WWW.downloadHandler.data,
                    x.Headers ?? new Dictionary<string, string>(), options));

        private IObservable<UnityWebRequest> RequestInternal(string url, Options options = null)
        {
            if (options == null || options.Method == HttpMethod.Get)
                return ObservableWebRequest.Get(url, options?.Headers);

            if (options.Method == HttpMethod.Post)
                return ObservableWebRequest.Post(url, options.PostForm, options.Headers);

            if (options.Method == HttpMethod.Put)
                return ObservableWebRequest.Put(url, options.PutBody, options.Headers);

            throw new NotImplementedException($"HTTP method {options.Method} not implemented for: {url}");
        }

        private string GetLogMessage(Uri uri, Options options)
        {
            var method = options?.Method ?? HttpMethod.Get;
            var headers = options?.Headers?.ToString() ?? "{}";
            
            if (method == HttpMethod.Get)
                return $"GET {uri}{NewLine}Headers:{NewLine}{headers}";

            if (method == HttpMethod.Post)
                return $"POST {uri}{NewLine}Form:{NewLine}{options?.PostForm}{NewLine}Headers:{NewLine}{headers}";

            if (method == HttpMethod.Put)
                return $"PUT {uri}{NewLine}Body:{NewLine}{options?.PutBody}{NewLine}Headers:{NewLine}{headers}";

            throw new NotImplementedException($"HTTP method {options?.Method} not implemented for: {uri}");
        }
    }
}