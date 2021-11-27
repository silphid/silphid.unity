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

        public IObservable<Response> Request(Uri uri, IOptions options = null) =>
            RequestInternal(uri.AbsoluteUri, options)
               .DoOnSubscribe(
                    () =>
                    {
                        if (Log.IsDebugEnabled)
                            Log.Debug(GetLogMessage(uri, options));
                    })
               .DoOnError(
                    ex =>
                    {
                        if (ex is NetworkException)
                            _status.Value = NetworkStatus.Offline;

                        if (Log.IsDebugEnabled)
                            Log.Debug($"Failed {GetLogMessage(uri, options)} with exception:\r\n{ex}");
                    })
               .Select(
                    www => new
                    {
                        WWW = www,
                        Headers = www.GetResponseHeaders()
                    })
               .Do(
                    x =>
                    {
                        if (uri.Scheme != Scheme.Http && uri.Scheme != Scheme.Https)
                            return;

                        if (x.Headers == null)
                            Log.Warn($"No headers in response from {uri}");

                        _status.Value = NetworkStatus.Online;
                    })
               .Select(
                    x => new Response(
                        x.WWW.responseCode,
                        x.Headers ?? new Dictionary<string, string>(),
                        () => x.WWW.downloadHandler?.data,
                        () => (x.WWW.downloadHandler as DownloadHandlerTexture)?.texture));

        private IObservable<UnityWebRequest> RequestInternal(string url, IOptions options = null)
        {
            if (Log.IsDebugEnabled)
                Log.Debug($"Requesting {url} with options: {options}");

            if (options.IsTextureMode())
                return ObservableWebRequest.GetTexture(url, options.GetHeaders(), options.GetHttpTimeout());

            var method = options.GetHttpMethod();

            if (method == HttpMethod.Get)
                return ObservableWebRequest.Get(url, options.GetHeaders(), options.GetHttpTimeout());
            
            if (method == HttpMethod.Delete)
                return ObservableWebRequest.Delete(url, options.GetHeaders(), options.GetHttpTimeout());

            var body = options.GetBody();

            // Review replace WWWForm by body
            if (method == HttpMethod.Post && body == null)
                return ObservableWebRequest.Post(
                    url,
                    options.GetWWWForm(),
                    options.GetHeaders(),
                    options.GetHttpTimeout());

            return ObservableWebRequest.Request(
                method.ToString()
                      .ToUpper(),
                url,
                body,
                options.GetHeaders(),
                options.GetHttpTimeout());
        }

        private string GetLogMessage(Uri uri, IOptions options)
        {
            var method = options.GetHttpMethod()
                                .ToString()
                                .ToUpper();

            return $"{method} {uri}";
        }
    }
}