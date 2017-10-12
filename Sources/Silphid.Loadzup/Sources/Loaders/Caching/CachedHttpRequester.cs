using System;
using System.Collections.Generic;
using System.Net;
using log4net;
using Silphid.Extensions;
using Silphid.Loadzup.Http;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup.Caching
{
    public class CachedHttpRequester : IHttpRequester
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CachedHttpRequester));

        public CachePolicy DefaultPolicy { get; set; } = CachePolicy.OriginOnly;

        private readonly IHttpRequester _requester;
        private readonly ICacheStorage _cacheStorage;

        public CachedHttpRequester(IHttpRequester requester, ICacheStorage cacheStorage)
        {
            _requester = requester;
            _cacheStorage = cacheStorage;
        }

        public IObservable<Response> Post(Uri uri, WWWForm form, Options options = null) =>
            _requester.Post(uri, form, options);

        public IObservable<Response> Get(Uri uri, Options options = null) =>
            Request(uri, options);

        public IObservable<Response> Put(Uri uri, string body, Options options = null) =>
            _requester.Put(uri, body, options);

        public IObservable<Response> Request(Uri uri, Options options = null)
        {
            var policy = options?.CachePolicy ?? DefaultPolicy;
            Log.Debug($"{policy} - {uri}");

            if (policy == CachePolicy.OriginOnly)
                return LoadFromOrigin(policy, uri, options);

            var responseHeaders = _cacheStorage.LoadHeaders(uri);
            if (responseHeaders != null)
            {
                // Cached
                Log.Debug($"{policy} - Resource found in cache: {uri}");

                if (policy == CachePolicy.CacheOnly || policy == CachePolicy.CacheOtherwiseOrigin)
                    return LoadFromCache(policy, uri, responseHeaders);

                if (policy == CachePolicy.OriginOtherwiseCache)
                    return LoadFromOrigin(policy, uri, options)
                        .Catch<Response, HttpException>(ex =>
                        {
                            Log.Debug(
                                $"{policy} - Failed to retrieve {uri} from origin (error: {ex}), falling back to cached version.");
                            return LoadFromCache(policy, uri, responseHeaders);
                        });

                if (policy == CachePolicy.CacheThenOrigin)
                    return LoadFromCacheThenOrigin(policy, uri, options, responseHeaders);

                if (policy == CachePolicy.OriginIfETagOtherwiseCache || policy == CachePolicy.CacheThenOriginIfETag)
                    return LoadWithETag(policy, uri, options, responseHeaders);

                if (policy == CachePolicy.OriginIfLastModifiedOtherwiseCache ||
                    policy == CachePolicy.CacheThenOriginIfLastModified)
                    return LoadWithLastModified(policy, uri, options, responseHeaders);
            }
            else
            {
                // Not cached
                Log.Debug($"{policy} - Resource not found in cache: {uri}");

                if (policy == CachePolicy.CacheOnly)
                    return Observable.Throw<Response>(
                        new InvalidOperationException($"Policy is {policy} but resource not found in cache"));
            }

            return LoadFromOrigin(policy, uri, options);
        }

        private IObservable<Response> LoadWithETag(CachePolicy policy, Uri uri, Options options,
            Dictionary<string, string> responseHeaders)
        {
            Log.Debug($"{policy} - LoadWithETag: {uri}");
            var eTag = responseHeaders.GetValueOrDefault(KnownHttpHeaders.ETag);
            if (eTag == null)
                return LoadFromOrigin(policy, uri, options);

            options.SetHeader(KnownHttpHeaders.IfNoneMatch, eTag);

            if (policy == CachePolicy.OriginIfETagOtherwiseCache)
                return LoadFromOrigin(policy, uri, options)
                    .Catch<Response, HttpException>(ex =>
                    {
                        Log.Debug(
                            $"{policy} - Failed to retrieve {uri} from origin (error: {ex}), falling back to cached version.");
                        return LoadFromCache(policy, uri, responseHeaders);
                    });

            // CacheThenOriginIfETag
            return LoadFromCacheThenOrigin(policy, uri, options, responseHeaders);
        }

        private IObservable<Response> LoadWithLastModified(CachePolicy policy, Uri uri, Options options,
            Dictionary<string, string> responseHeaders)
        {
            Log.Debug($"{policy} - LoadWithLastModified: {uri}");
            var lastModified = responseHeaders.GetValueOrDefault(KnownHttpHeaders.LastModified);
            if (lastModified == null)
                return LoadFromCacheThenOrigin(policy, uri, options, responseHeaders);

            options.SetHeader(KnownHttpHeaders.IfModifiedSince, lastModified);

            if (policy == CachePolicy.OriginIfETagOtherwiseCache)
                return LoadFromOrigin(policy, uri, options)
                    .Catch<Response, HttpException>(ex =>
                    {
                        Log.Debug(
                            $"{policy} - Failed to retrieve {uri} from origin (Status: {ex.StatusCode}, Error: {ex}), falling back to cached version.");
                        return LoadFromCache(policy, uri, responseHeaders);
                    });

            // CacheThenOriginIfLastModified
            return LoadFromCacheThenOrigin(policy, uri, options, responseHeaders);
        }

        private IObservable<Response> LoadFromCacheThenOrigin(CachePolicy policy, Uri uri, Options options,
            Dictionary<string, string> responseHeaders)
        {
            Log.Debug($"{policy} - LoadFromCacheThenOrigin: {uri}");
            return LoadFromCache(policy, uri, responseHeaders)
                .Concat(Observable.Defer(() => LoadFromOrigin(policy, uri, options)
                    .Catch<Response, HttpException>(ex => ex.StatusCode == HttpStatusCode.NotModified
                        ? Observable.Empty<Response>()
                        : Observable.Throw<Response>(ex))));
        }

        private IObservable<Response> LoadFromCache(CachePolicy policy, Uri uri,
            Dictionary<string, string> responseHeaders)
        {
            Log.Debug($"{policy} - Loading resource from cache: {uri}");
            return Observable.Return(new Response(_cacheStorage.Load(uri), responseHeaders));
        }

        private IObservable<Response> LoadFromOrigin(CachePolicy policy, Uri uri, Options options)
        {
            Log.Debug($"{policy} - Loading resource from origin: {uri}");
            return _requester
                .Request(uri, options)
                .Do(x =>
                {
                    string statusCode;
                    if (x.Headers.TryGetValue(KnownHttpHeaders.Status, out statusCode))
                    {
                        var code = statusCode.Split(' ');
                        if (code.Length >= 2 && code[1] == ((int) HttpStatusCode.NotModified).ToString())
                            throw new HttpException(HttpStatusCode.NotModified);
                    }

                    if (policy != CachePolicy.OriginOnly)
                        _cacheStorage.Save(uri, x.Bytes, x.Headers);
                });
        }
    }
}