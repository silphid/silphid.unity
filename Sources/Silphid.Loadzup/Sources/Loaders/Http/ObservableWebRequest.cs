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
        public static IObservable<UnityWebRequest> Request(UnityWebRequest request)
        {
            return Observable.FromCoroutine<UnityWebRequest>((observer, cancellation) =>
                Fetch(request, observer));
        }

        public static IObservable<UnityWebRequest> Get(string url, Dictionary<string, string> headers = null)
        {
            var request = UnityWebRequest.Get(url);
            headers?.ForEach(x => request.SetRequestHeader(x.Key, x.Value));
            return Request(request);
        }
        
        public static IObservable<UnityWebRequest> Post(string url, WWWForm form, Dictionary<string, string> headers = null)
        {
            var request = UnityWebRequest.Post(url, form);
            headers?.ForEach(x => request.SetRequestHeader(x.Key, x.Value));
            return Request(request);
        }
        
        public static IObservable<UnityWebRequest> Put(string url, string body, Dictionary<string, string> headers = null)
        {
            var request = UnityWebRequest.Put(url, body);
            headers?.ForEach(x => request.SetRequestHeader(x.Key, x.Value));
            return Request(request);
        }


        private static IEnumerator Fetch(UnityWebRequest www, IObserver<UnityWebRequest> observer)
        {
            using (www)
            {
                yield return www.Send();
                if (!www.error.IsNullOrEmpty())
                {
                    observer.OnError(new Exception(www.downloadHandler.text));
                }
                else
                {
                    observer.OnNext(www);
                    observer.OnCompleted();
                }
            }
        }

        private static IEnumerator FetchExemple(UnityWebRequest www, IObserver<byte[]> observer, IProgress<float> reportProgress, CancellationToken cancel)
        {
            using (www)
            {
                if (reportProgress != null)
                {
                    while (!www.isDone && !cancel.IsCancellationRequested)
                    {
                        try
                        {
                            reportProgress.Report(www.downloadProgress);
                        }
                        catch (Exception ex)
                        {
                            observer.OnError(ex);
                            yield break;
                        }
                        yield return null;
                    }
                }
                else
                {
                    if (!www.isDone)
                    {
                        yield return www;
                    }
                }

                if (cancel.IsCancellationRequested)
                {
                    yield break;
                }

                if (reportProgress != null)
                {
                    try
                    {
                        reportProgress.Report(www.downloadProgress);
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                        yield break;
                    }
                }

                if (!string.IsNullOrEmpty(www.error))
                {
                    observer.OnError(new RequestException(www));
                }
                else
                {
                    observer.OnNext(www.downloadHandler.data);
                    observer.OnCompleted();
                }
            }
        }
    }
}