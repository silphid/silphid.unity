using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Silphid.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Silphid.Loadzup.Http
{
    public static class ObservableWebRequest
    {
        public static IObservable<UnityWebRequest> Get(string url,
                                                       IDictionary<string, string> headers = null,
                                                       TimeSpan? timeout = null) =>
            Observable.Defer(
                () =>
                {
                    var request = UnityWebRequest.Get(url);
                    SetTimeout(request, timeout);
                    headers?.ForEach(x => request.SetRequestHeader(x.Key, x.Value));
                    return Send(request);
                });

        public static IObservable<UnityWebRequest> Post(string url,
                                                        WWWForm form,
                                                        IDictionary<string, string> headers = null,
                                                        TimeSpan? timeout = null) =>
            Observable.Defer(
                () =>
                {
                    var request = UnityWebRequest.Post(url, form);
                    SetTimeout(request, timeout);
                    headers?.ForEach(x => request.SetRequestHeader(x.Key, x.Value));
                    return Send(request);
                });

        public static IObservable<UnityWebRequest> Post(string url,
                                                        string body,
                                                        IDictionary<string, string> headers = null,
                                                        TimeSpan? timeout = null) =>
            Observable.Defer(
                () =>
                {
                    var request = UnityWebRequest.Post(url, body);
                    SetTimeout(request, timeout);
                    headers?.ForEach(x => request.SetRequestHeader(x.Key, x.Value));
                    return Send(request);
                });

        public static IObservable<UnityWebRequest> Delete(string url,
                                                       IDictionary<string, string> headers = null,
                                                       TimeSpan? timeout = null) =>
            Observable.Defer(
                () =>
                {
                    var request = UnityWebRequest.Delete(url);
                    SetTimeout(request, timeout);
                    headers?.ForEach(x => request.SetRequestHeader(x.Key, x.Value));
                    return Send(request);
                });
        
        public static IObservable<UnityWebRequest> Request(string method,
                                                           string url,
                                                           string body,
                                                           IDictionary<string, string> headers = null,
                                                           TimeSpan? timeout = null)
        {
            return Request(method, url, Encoding.UTF8.GetBytes(body), headers, timeout);
        }

        public static IObservable<UnityWebRequest> Request(string method,
                                                           string url,
                                                           byte[] bodyRaw,
                                                           IDictionary<string, string> headers = null,
                                                           TimeSpan? timeout = null)
        {
            return Observable.Defer(
                () =>
                {
                    var request = new UnityWebRequest(url, method);
                    SetTimeout(request, timeout);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    headers?.ForEach(x => request.SetRequestHeader(x.Key, x.Value));
                    return Send(request);
                });
        }

        public static IObservable<UnityWebRequest> Put(string url,
                                                       string body,
                                                       IDictionary<string, string> headers = null,
                                                       TimeSpan? timeout = null) =>
            Observable.Defer(
                () =>
                {
                    var request = UnityWebRequest.Put(url, body);
                    SetTimeout(request, timeout);
                    headers?.ForEach(x => request.SetRequestHeader(x.Key, x.Value));
                    return Send(request);
                });

        public static IObservable<UnityWebRequest> GetTexture(string url,
                                                              IDictionary<string, string> headers = null,
                                                              TimeSpan? timeout = null) =>
            Observable.Defer(
                () =>
                {
                    var request = UnityWebRequestTexture.GetTexture(url);
                    SetTimeout(request, timeout);
                    headers?.ForEach(x => request.SetRequestHeader(x.Key, x.Value));

                    return Send(request)
                       .Do(
                            x =>
                            {
                                var texture = (x.downloadHandler as DownloadHandlerTexture)?.texture;

                                if (texture != null)
                                    texture.wrapMode = TextureWrapMode.Clamp;
                            });
                });

        private static void SetTimeout(UnityWebRequest request, TimeSpan? timeout)
        {
            request.timeout = timeout?.TotalSeconds.RoundToInt() ?? 0;
        }

        private static IObservable<UnityWebRequest> Send(UnityWebRequest request) =>
            Observable.FromCoroutine<UnityWebRequest>((observer, cancellation) => Send(request, observer));

        private static IEnumerator Send(UnityWebRequest webRequest, IObserver<UnityWebRequest> observer)
        {
            using (webRequest)
            {
                AsyncOperation operation;

                try
                {
                    operation = webRequest.SendWebRequest();
                }
                catch (Exception exception)
                {
                    observer.OnError(new Exception($"UnityWebRequest.Send() failed for {webRequest.url}", exception));
                    yield break;
                }

                yield return operation;

                // Error?
                if (webRequest.isNetworkError)
                    observer.OnError(new NetworkException(webRequest));
                else if (webRequest.isHttpError)
                    observer.OnError(new HttpException(webRequest));
                else
                {
                    // Success
                    observer.OnNext(webRequest);
                    observer.OnCompleted();
                }
            }
        }
    }
}