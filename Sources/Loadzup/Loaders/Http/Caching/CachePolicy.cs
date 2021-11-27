namespace Silphid.Loadzup.Http.Caching
{
    public enum CachePolicy
    {
        /// <summary>
        /// Specifies that resource should be loaded only from origin.  Failure to retrieve resource from origin results
        /// in an error and no further step is performed.
        /// </summary>
        Origin,

        /// <summary>
        /// Specifies that resource should be loaded from cache, and only if not found there should it be loaded from
        /// origin.  This is the fastest approach, but does not allow updates on the server to be retrieved.
        /// </summary>
        CacheElseOrigin,

        /// <summary>
        /// Specifies that resource should be loaded from origin first, and only if a NetworkException occurs should it
        /// then be loaded from cache.  This is useful for offline scenarios, where latest version of resource should
        /// ideally be downloaded, but application wants to fallback to version in cache if connectivity is not
        /// available.  This approach provides no speed gain, only offline support, and allows updates on the server to
        /// be retrieved.
        /// </summary>
        OriginElseCache,

        /// <summary>
        /// Specifies that cache should be queried first for a quick (though possibly outdated) version of data, but
        /// then origin should always be queried anyway for a potentially more up-to-date version of data, which will
        /// always be downloaded, without checking for changes with ETag or Last-Modified. This a great compromise for
        /// fast startup time versus up-to-date data, as long as consumer is able to handle multiple versions of
        /// data through subsequent invocations of OnNext().
        /// </summary>
        CacheThenOrigin,

        /// <summary>
        /// Specifies that cache should be queried first for a quick (though possibly outdated) version of data, but
        /// then origin should always be queried anyway for a potentially more up-to-date version of data, which will
        /// only be downloaded if its ETag is different from its cached counterpart. This a great compromise for fast
        /// startup time versus up-to-date data, as long as consumer is able to handle potentially multiple versions of
        /// data through subsequent invocations of OnNext().
        /// </summary>
        CacheThenOriginIfETag,

        /// <summary>
        /// Specifies that cache should be queried first for a quick (though possibly outdated) version of data, but
        /// then origin should always be queried anyway for a potentially more up-to-date version of data, which will
        /// only be downloaded if its Last-Modified date is more recent than its cached counterpart. This a great
        /// compromise for fast startup time versus up-to-date data, as long as consumer is able to handle potentially
        /// multiple versions of data through subsequent invocations of OnNext().
        /// </summary>
        CacheThenOriginIfLastModified,

        /// <summary>
        /// Specifies that system should first check with server for a more recent version of the resource, using HTTP's
        /// ETag mecanism, and only otherwise fallback to cache or if download fails altogether.
        /// </summary>
        OriginIfETagElseCache,

        /// <summary>
        /// Specifies that system should first check with server for a more recent version of the resource, using HTTP's
        /// Last-Modified mecanism, and only otherwise fallback to cache or if download fails altogether.
        /// </summary>
        OriginIfLastModifiedElseCache
    }
}