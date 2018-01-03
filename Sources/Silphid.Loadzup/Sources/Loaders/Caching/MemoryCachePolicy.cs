namespace Silphid.Loadzup.Caching
{
    public enum MemoryCachePolicy
    {
        /// <summary>
        /// Specifies that resource should be loaded only from origin.  Failure to retrieve resource from origin results
        /// in an error and no further step is performed.
        /// </summary>
        OriginOnly,

        /// <summary>
        /// Specifies that resource should be loaded from cache, and only if not found there should it be loaded from
        /// origin.
        /// </summary>
        CacheOtherwiseOrigin
    }
}