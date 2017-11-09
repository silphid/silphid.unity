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
        public static IObservable<UnityWebRequest> Get(string url, IDictionary<string, string> headers = null)
        {
            return Observable.Defer(() =>
            {
                var request = UnityWebRequest.Get(url);
                headers?.ForEach(x => request.SetRequestHeader(x.Key, x.Value));
                return Request(request);
            });
        }
        
        public static IObservable<UnityWebRequest> Post(string url, WWWForm form, IDictionary<string, string> headers = null)
        {
            return Observable.Defer(() =>
            {
                var request = UnityWebRequest.Post(url, form);
                headers?.ForEach(x => request.SetRequestHeader(x.Key, x.Value));
                return Request(request);
            });
        }
        
        public static IObservable<UnityWebRequest> Put(string url, string body, IDictionary<string, string> headers = null)
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