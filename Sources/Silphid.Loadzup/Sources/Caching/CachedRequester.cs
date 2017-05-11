using System;
using System.Collections.Generic;
using System.Net;
using Silphid.Extensions;
using Silphid.Loadzup.Http;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup.Caching
{
    public class CachedRequester : IRequester
    {
        public CachePolicy DefaultPolicy { get; set; } = CachePolicy.OriginOnly;

        private readonly IRequester _requester;
        private readonly ICacheStorage _cacheStorage;

        public CachedRequester(IRequester requester, ICacheStorage cacheStorage)
        {
            _requester = requester;
            _cacheStorage = cacheStorage;
        }

        public UniRx.IObservable<Response> Request(Uri uri, Options options = null)
        {
            var policy = options?.CachePolicy ?? DefaultPolicy;
            //Debug.Log($"#Loadzup# Request({uri}, {policy})");

            if (policy == CachePolicy.OriginOnly)
                return LoadFromOrigin(policy, uri, options);

            var responseHeaders = _cacheStorage.LoadHeaders(uri);
            if (responseHeaders != null)
            {
                // Cached
                //Debug.Log($"#Loadzup# {policy} - Resource found in cache: {uri}");

                if (policy == CachePolicy.CacheOnly || policy == CachePolicy.CacheOtherwiseOrigin)
                    return LoadFromCache(uri, responseHeaders);

                if (policy == CachePolicy.OriginOtherwiseCache)
                    return LoadFromOrigin(policy, uri, options)
                        .Catch<Response, RequestException>(ex =>
                        {
                          //  Debug.Log($"#Loadzup# {policy} - Failed to retrieve {uri} from origin (error: {ex}), falling back to cached version.");
                            return LoadFromCache(uri, responseHeaders);
                        });

                if (policy == CachePolicy.CacheThenOrigin)
                    return LoadFromCacheThenOrigin(policy, uri, options, responseHeaders);

                if (policy == CachePolicy.OriginIfETagOtherwiseCache || policy == CachePolicy.CacheThenOriginIfETag)
                    return LoadWithETag(policy, uri, options, responseHeaders);

                if (policy == CachePolicy.OriginIfLastModifiedOtherwiseCache || policy == CachePolicy.CacheThenOriginIfLastModified)
                    return LoadWithLastModified(policy, uri, options, responseHeaders);
            }
            else
            {
                // Not cached
              //  Debug.Log($"#Loadzup# {policy} - Resource not found in cache: {uri}");

                if (policy == CachePolicy.CacheOnly)
                    return Observable.Throw<Response>(new InvalidOperationException($"Policy is {policy} but resource not found in cache"));
            }

            return LoadFromOrigin(policy, uri, options);
        }

        private UniRx.IObservable<Response> LoadWithETag(CachePolicy policy, Uri uri, Options options, Dictionary<string, string> responseHeaders)
        {
            //Debug.Log($"#Loadzup# LoadWithETag");
            var eTag = responseHeaders.GetOptionalValue(KnownHttpHeaders.ETag);
            if (eTag == null)
                return LoadFromOrigin(policy, uri, options);

            options.SetRequestHeader(KnownHttpHeaders.IfNoneMatch, eTag);

            if (policy == CachePolicy.OriginIfETagOtherwiseCache)
                return LoadFromOrigin(policy, uri, options)
                    .Catch<Response, RequestException>(ex =>
                    {
                        //Debug.Log($"#Loadzup# {policy} - Failed to retrieve {uri} from origin (error: {ex}), falling back to cached version.");
                        return LoadFromCache(uri, responseHeaders);
                    });

            // CacheThenOriginIfETag
            return LoadFromCacheThenOrigin(policy, uri, options, responseHeaders);
        }

        private UniRx.IObservable<Response> LoadWithLastModified(CachePolicy policy, Uri uri, Options options, Dictionary<string, string> responseHeaders)
        {
           // Debug.Log($"#Loadzup# LoadWithLastModified");
            var lastModified = responseHeaders.GetOptionalValue(KnownHttpHeaders.LastModified);
            if (lastModified == null)
                return LoadFromCacheThenOrigin(policy, uri, options, responseHeaders);

            options.SetRequestHeader(KnownHttpHeaders.IfModifiedSince, lastModified);

            if (policy == CachePolicy.OriginIfETagOtherwiseCache)
                return LoadFromOrigin(policy, uri, options)
                    .Catch<Response, RequestException>(ex =>
                    {
                        Debug.Log($"#Loadzup# {policy} - Failed to retrieve {uri} from origin (Status: {ex.StatusCode}, Error: {ex}), falling back to cached version.");
                        return LoadFromCache(uri, responseHeaders);
                    });

            // CacheThenOriginIfLastModified
            return LoadFromCacheThenOrigin(policy, uri, options, responseHeaders);
        }

        private UniRx.IObservable<Response> LoadFromCacheThenOrigin(CachePolicy policy, Uri uri, Options options, Dictionary<string, string> responseHeaders)
        {
           // Debug.Log($"#Loadzup# LoadFromCacheThenOrigin");
            return LoadFromCache(uri, responseHeaders)
                .Concat(Observable.Defer(() => LoadFromOrigin(policy, uri, options)
                    .Catch<Response, RequestException>(ex => ex.StatusCode == HttpStatusCode.NotModified
                        ? Observable.Empty<Response>()
                        : Observable.Throw<Response>(ex))));
        }

        private UniRx.IObservable<Response> LoadFromCache(Uri uri, Dictionary<string, string> responseHeaders)
        {
            //Debug.Log($"#Loadzup# {policy} - Loading resource from cache: {uri}");
            return Observable.Return(new Response(_cacheStorage.Load(uri), responseHeaders));
        }

        private UniRx.IObservable<Response> LoadFromOrigin(CachePolicy policy, Uri uri, Options options)
        {
            //Debug.Log($"#Loadzup# {policy} - Loading resource from origin: {uri}");
            return _requester
                .Request(uri, options)
                .Do(x =>
                {
                    string statusCode;
                    if (x.Headers.TryGetValue(KnownHttpHeaders.Status, out statusCode))
                    {
                        var code = statusCode.Split(' ');
                        if (code.Length >= 2 && code[1] == ((int)HttpStatusCode.NotModified).ToString())
                            throw new RequestException(HttpStatusCode.NotModified);
                    }

                    if (policy != CachePolicy.OriginOnly)
                        _cacheStorage.Save(uri, x.Bytes, x.Headers);
                });
        }
    }
}