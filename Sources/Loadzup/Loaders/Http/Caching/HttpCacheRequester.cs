using System;
using System.Net;
using System.Net.Http.Headers;
using log4net;
using UniRx;

namespace Silphid.Loadzup.Http.Caching
{
    public class HttpCacheRequester : IHttpRequester
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpCacheRequester));

        private readonly IHttpRequester _inner;
        private readonly IHttpCache _httpCache;
        private readonly CachePolicy _defaultPolicy;

        public HttpCacheRequester(IHttpRequester inner, IHttpCache httpCache, CachePolicy? defaultPolicy = null)
        {
            _inner = inner;
            _httpCache = httpCache;
            _defaultPolicy = defaultPolicy ?? CachePolicy.Origin;
        }

        public IObservable<Response> Request(Uri uri, IOptions options = null)
        {
            // Only GET method should be cached, delegate others to inner requester
            var method = options.GetHttpMethod();
            if (method != HttpMethod.Get)
                return _inner.Request(uri, options);

            var policy = options.GetCachePolicy(_defaultPolicy);

            if (policy == CachePolicy.Origin)
                return LoadFromOrigin(policy, uri, options);

            var entry = GetCacheEntry(uri, policy);
            if (entry != null)
            {
                if (policy == CachePolicy.CacheElseOrigin)
                    return LoadFromCache(policy, options, entry);

                if (policy == CachePolicy.OriginElseCache)
                    return LoadFromOrigin(policy, uri, options)
                       .Catch<Response, NetworkException>(
                            ex =>
                            {
                                Log.Debug(
                                    $"{policy} - {uri} - Failed to retrieve from origin (error: {ex}), falling back to cached version.");
                                return LoadFromCache(policy, options, entry);
                            });

                if (policy == CachePolicy.CacheThenOrigin)
                    return LoadFromCacheThenOrigin(policy, options, entry);

                if (policy == CachePolicy.OriginIfETagElseCache || policy == CachePolicy.CacheThenOriginIfETag)
                    return LoadWithETag(policy, options, entry);

                if (policy == CachePolicy.OriginIfLastModifiedElseCache ||
                    policy == CachePolicy.CacheThenOriginIfLastModified)
                    return LoadWithLastModified(policy, options, entry);
            }

            return LoadFromOrigin(policy, uri, options);
        }

        private HttpCacheEntry GetCacheEntry(Uri uri, CachePolicy policy)
        {
            var entry = _httpCache.GetEntry(uri);
            if (entry != null && !_httpCache.IsValid(entry))
            {
                Log.Debug($"{policy} - {uri} - Cached file EXPIRED");
                entry.Delete();
                return null;
            }

            return entry;
        }

        private IObservable<Response> LoadWithETag(CachePolicy policy, IOptions options, HttpCacheEntry entry)
        {
            Log.Debug($"{policy} - {entry.Uri} - LoadWithETag");
            var eTag = entry.Headers.ETag;
            if (eTag == null)
                return LoadFromOrigin(policy, entry.Uri, options);

            options = options.WithHeader(KnownHeaders.IfNoneMatch, eTag);

            if (policy == CachePolicy.OriginIfETagElseCache)
                return LoadFromOrigin(policy, entry.Uri, options)
                   .Catch<Response, HttpException>(
                        ex =>
                        {
                            Log.Debug(
                                $"{policy} - {entry.Uri} - Failed to retrieve from origin (error: {ex}), falling back to cached version.");
                            return LoadFromCache(policy, options, entry);
                        });

            // CacheThenOriginIfETag
            return LoadFromCacheThenOrigin(policy, options, entry);
        }

        private IObservable<Response> LoadWithLastModified(CachePolicy policy, IOptions options, HttpCacheEntry entry)
        {
            Log.Debug($"{policy} - {entry.Uri} - LoadWithLastModified");
            var lastModified = entry.Headers.LastModified;
            if (lastModified == null)
                return LoadFromCacheThenOrigin(policy, options, entry);

            options = options.WithHeader(KnownHeaders.IfModifiedSince, lastModified.ToString());

            if (policy == CachePolicy.OriginIfETagElseCache)
                return LoadFromOrigin(policy, entry.Uri, options)
                   .Catch<Response, HttpException>(
                        ex =>
                        {
                            Log.Debug(
                                $"{policy} - {entry.Uri} - Failed to retrieve from origin (Status: {ex.StatusCode}, Error: {ex}), falling back to cached version.");
                            return LoadFromCache(policy, options, entry);
                        });

            // CacheThenOriginIfLastModified
            return LoadFromCacheThenOrigin(policy, options, entry);
        }

        private IObservable<Response> LoadFromCacheThenOrigin(CachePolicy policy,
                                                              IOptions options,
                                                              HttpCacheEntry entry)
        {
            return LoadFromCache(policy, options, entry)
               .Concat(
                    Observable.Defer(
                        () => LoadFromOrigin(policy, entry.Uri, options)
                           .Catch<Response, HttpException>(
                                ex => ex.StatusCode == HttpStatusCode.NotModified
                                          ? Observable.Empty<Response>()
                                          : Observable.Throw<Response>(ex))));
        }

        private IObservable<Response> LoadFromCache(CachePolicy policy, IOptions options, HttpCacheEntry entry)
        {
            Log.Debug($"{policy} - {entry.Uri} - Loading from CACHE");

            return options.IsTextureMode()
                       ? _httpCache.LoadTexture(entry.Uri)
                                   .Select(x => new Response(KnownStatusCode.Ok, entry.Headers, null, () => x))
                       : _httpCache.Load(entry.Uri)
                                   .Select(bytes => new Response(KnownStatusCode.Ok, entry.Headers, () => bytes));
        }

        private IObservable<Response> LoadFromOrigin(CachePolicy policy, Uri uri, IOptions options)
        {
            Log.Debug($"{policy} - {uri} - Loading from ORIGIN");
            return _inner.Request(uri, options)
                         .Do(
                              response =>
                              {
                                  if (response.StatusCode == KnownStatusCode.NotModified)
                                      throw new HttpException(uri, HttpStatusCode.NotModified);

                                  var noCache = response.Headers?.CacheControl?.NoCache ?? false;
                                  if (!noCache && policy != CachePolicy.Origin)
                                  {
                                      SetCacheHeaders(response, policy, options);
                                      _httpCache.Save(uri, response.Bytes, response.Headers);
                                  }
                              });
        }

        private void SetCacheHeaders(Response response, CachePolicy policy, IOptions options)
        {
            var timeToLive = options.GetTimeToLive();
            if (timeToLive != null)
            {
                if (response.Headers.CacheControl == null)
                    response.Headers.CacheControl = new CacheControlHeaderValue();

                if (response.Headers.CacheControl.MaxAge == null)
                    response.Headers.CacheControl.MaxAge = timeToLive;
            }

            var cacheGroup = options.GetCacheGroup();
            if (cacheGroup != null)
                response.Headers.E2CacheGroup = cacheGroup.Name;
        }
    }
}