using System;
using System.Collections.Generic;
using log4net;
using UniRx;
using UnityEngine.Networking;

namespace Silphid.Loadzup.Http
{
    public class HttpRequester : IHttpRequester, INetworkStatusProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpRequester));

        private readonly ReactiveProperty<NetworkStatus> _status =
            new ReactiveProperty<NetworkStatus>(NetworkStatus.Undetermined);

        public IReadOnlyReactiveProperty<NetworkStatus> Status => _status;

        public IObservable<Response> Request(Uri uri, Options options = null) =>
            RequestInternal(uri.AbsoluteUri, options)
                .DoOnSubscribe(() =>
                    Log.Info(GetLogMessage(uri, options)))
                .DoOnError(ex =>
                {
                    if (ex is NetworkException)
                        _status.Value = NetworkStatus.Offline;
                    
                    Log.Info($"Failed {GetLogMessage(uri, options)}", ex);
                })
                .Select(www => new
                {
                    WWW = www,
                    Headers = www.GetResponseHeaders()
                })
                .Do(x =>
                {
                    if (uri.Scheme != Scheme.Http && uri.Scheme != Scheme.Https)
                        return;

                    if (x.Headers == null)
                        Log.Warn($"No headers in response from {uri}");
                        
                    _status.Value = NetworkStatus.Online;
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
            var method = (options?.Method ?? HttpMethod.Get)
                .ToString()
                .ToUpper();
            
            return $"{method} {uri}";
        }
    }
}