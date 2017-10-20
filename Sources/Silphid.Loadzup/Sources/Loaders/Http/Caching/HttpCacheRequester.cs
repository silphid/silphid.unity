using System;
using System.Collections.Generic;
using System.Net;
using log4net;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Loadzup.Http.Caching
{
    public class HttpCacheRequester : IHttpRequester
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpCacheRequester));

        private readonly IHttpRequester _requester;
        private readonly IHttpCache _httpCache;
        private readonly HttpCachePolicy _defaultPolicy;

        public HttpCacheRequester(IHttpRequester requester, IHttpCache httpCache, HttpCachePolicy? defaultPolicy = null)
        {
            _requester = requester;
            _httpCache = httpCache;
            _defaultPolicy = defaultPolicy ?? HttpCachePolicy.OriginOnly;
        }

        public IObservable<Response> Request(Uri uri, Options options = null)
        {
            var policy = options?.HttpCachePolicy ?? _defaultPolicy;
            Log.Debug($"{policy} - {uri}");

            if (policy == HttpCachePolicy.OriginOnly)
                return LoadFromOrigin(policy, uri, options);

            var responseHeaders = _httpCache.LoadHeaders(uri);
            if (responseHeaders != null)
            {
                // Cached
                Log.Debug($"{policy} - Resource found in cache: {uri}");

                if (policy == HttpCachePolicy.CacheOnly || policy == HttpCachePolicy.CacheOtherwiseOrigin)
                    return LoadFromCache(policy, uri, responseHeaders);

                if (policy == HttpCachePolicy.OriginOtherwiseCache)
                    return LoadFromOrigin(policy, uri, options)
                        .Catch<Response, HttpException>(ex =>
                        {
                            Log.Debug(
                                $"{policy} - Failed to retrieve {uri} from origin (error: {ex}), falling back to cached version.");
                            return LoadFromCache(policy, uri, responseHeaders);
                        });

                if (policy == HttpCachePolicy.CacheThenOrigin)
                    return LoadFromCacheThenOrigin(policy, uri, options, responseHeaders);

                if (policy == HttpCachePolicy.OriginIfETagOtherwiseCache || policy == HttpCachePolicy.CacheThenOriginIfETag)
                    return LoadWithETag(policy, uri, options, responseHeaders);

                if (policy == HttpCachePolicy.OriginIfLastModifiedOtherwiseCache ||
                    policy == HttpCachePolicy.CacheThenOriginIfLastModified)
                    return LoadWithLastModified(policy, uri, options, responseHeaders);
            }
            else
            {
                // Not cached
                Log.Debug($"{policy} - Resource not found in cache: {uri}");

                if (policy == HttpCachePolicy.CacheOnly)
                    return Observable.Throw<Response>(
                        new InvalidOperationException($"Policy is {policy} but resource not found in cache"));
            }

            return LoadFromOrigin(policy, uri, options);
        }

        private IObservable<Response> LoadWithETag(HttpCachePolicy policy, Uri uri, Options options,
            Dictionary<string, string> responseHeaders)
        {
            Log.Debug($"{policy} - LoadWithETag: {uri}");
            var eTag = responseHeaders.GetValueOrDefault(KnownHttpHeaders.ETag);
            if (eTag == null)
                return LoadFromOrigin(policy, uri, options);

            options.SetHeader(KnownHttpHeaders.IfNoneMatch, eTag);

            if (policy == HttpCachePolicy.OriginIfETagOtherwiseCache)
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

        private IObservable<Response> LoadWithLastModified(HttpCachePolicy policy, Uri uri, Options options,
            Dictionary<string, string> responseHeaders)
        {
            Log.Debug($"{policy} - LoadWithLastModified: {uri}");
            var lastModified = responseHeaders.GetValueOrDefault(KnownHttpHeaders.LastModified);
            if (lastModified == null)
                return LoadFromCacheThenOrigin(policy, uri, options, responseHeaders);

            options.SetHeader(KnownHttpHeaders.IfModifiedSince, lastModified);

            if (policy == HttpCachePolicy.OriginIfETagOtherwiseCache)
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

        private IObservable<Response> LoadFromCacheThenOrigin(HttpCachePolicy policy, Uri uri, Options options,
            Dictionary<string, string> responseHeaders)
        {
            Log.Debug($"{policy} - LoadFromCacheThenOrigin: {uri}");
            return LoadFromCache(policy, uri, responseHeaders)
                .Concat(Observable.Defer(() => LoadFromOrigin(policy, uri, options)
                    .Catch<Response, HttpException>(ex => ex.StatusCode == HttpStatusCode.NotModified
                        ? Observable.Empty<Response>()
                        : Observable.Throw<Response>(ex))));
        }

        private IObservable<Response> LoadFromCache(HttpCachePolicy policy, Uri uri,
            Dictionary<string, string> responseHeaders)
        {
            Log.Debug($"{policy} - Loading resource from cache: {uri}");
            return _httpCache
                .Load(uri)
                .Select(bytes => new Response(KnownStatusCode.Ok, bytes, responseHeaders));
        }

        private IObservable<Response> LoadFromOrigin(HttpCachePolicy policy, Uri uri, Options options)
        {
            Log.Debug($"{policy} - Loading resource from origin: {uri}");
            return _requester
                .Request(uri, options)
                .Do(x =>
                {
                    if (x.StatusCode == KnownStatusCode.NotModified)
                        throw new HttpException(uri, HttpStatusCode.NotModified);

                    if (policy != HttpCachePolicy.OriginOnly)
                        _httpCache.Save(uri, x.Bytes, x.Headers);
                });
        }
    }
}