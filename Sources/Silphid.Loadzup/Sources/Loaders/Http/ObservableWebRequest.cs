using System;
using System.Collections;
using System.Collections.Generic;
using Silphid.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Silphid.Loadzup.Http
{
    public static class ObservableWebRequest
    {
        public static IObservable<UnityWebRequest> Get(string url, Dictionary<string, string> headers = null)
        {
            return Observable.Defer(() =>
            {
                var request = UnityWebRequest.Get(url);
                headers?.ForEach(x => request.SetRequestHeader(x.Key, x.Value));
                return Request(request);
            });
        }
        
        public static IObservable<UnityWebRequest> Post(string url, WWWForm form, Dictionary<string, string> headers = null)
        {
            return Observable.Defer(() =>
            {
                var request = UnityWebRequest.Post(url, form);
                headers?.ForEach(x => request.SetRequestHeader(x.Key, x.Value));
                return Request(request);
            });
        }
        
        public static IObservable<UnityWebRequest> Put(string url, string body, Dictionary<string, string> headers = null)
        {
            return Observable.Defer(() =>
            {
                var request = UnityWebRequest.Put(url, body);
                headers?.ForEach(x => request.SetRequestHeader(x.Key, x.Value));
                return Request(request);
            });
        }
        
        private static IObservable<UnityWebRequest> Request(UnityWebRequest request)
        {
            return Observable.FromCoroutine<UnityWebRequest>((observer, cancellation) =>
                Fetch(request, observer));
        }

        private static IEnumerator Fetch(UnityWebRequest webRequest, IObserver<UnityWebRequest> observer)
        {
            using (webRequest)
            {
                AsyncOperation operation;
                
                try
                {
                    operation = webRequest.Send();
                }
                catch (Exception exception)
                {
                    observer.OnError(new Exception($"UnityWebRequest.Send() failed for {webRequest.url}", exception));
                    yield break;
                }
                
                yield return operation;
                
                // Error?
                if (webRequest.isNetworkError)
                    observer.OnError(new NetworkException(webRequest.error));
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

// Original implementation :
//
//        private static IEnumerator Fetch(UnityWebRequest www, IObserver<byte[]> observer, IProgress<float> reportProgress, CancellationToken cancel)
//        {
//            using (www)
//            {
//                if (reportProgress != null)
//                {
//                    while (!www.isDone && !cancel.IsCancellationRequested)
//                    {
//                        try
//                        {
//                            reportProgress.Report(www.downloadProgress);
//                        }
//                        catch (Exception ex)
//                        {
//                            observer.OnError(ex);
//                            yield break;
//                        }
//                        yield return null;
//                    }
//                }
//                else
//                {
//                    if (!www.isDone)
//                    {
//                        yield return www;
//                    }
//                }
//
//                if (cancel.IsCancellationRequested)
//                {
//                    yield break;
//                }
//
//                if (reportProgress != null)
//                {
//                    try
//                    {
//                        reportProgress.Report(www.downloadProgress);
//                    }
//                    catch (Exception ex)
//                    {
//                        observer.OnError(ex);
//                        yield break;
//                    }
//                }
//
//                if (!string.IsNullOrEmpty(www.error))
//                {
//                    observer.OnError(new HttpException(www));
//                }
//                else
//                {
//                    observer.OnNext(www.downloadHandler.data);
//                    observer.OnCompleted();
//                }
//            }
//        }
    }
}