using System;
using Silphid.Loadzup.Http.Caching;

namespace Silphid.Loadzup.Loaders.Http.Caching
{
    public class CacheGroupFilter : IFilter
    {
        private readonly CacheGroup _cacheGroup;
        private readonly CachePolicy? _policy;
        private readonly TimeSpan? _timeToLive;

        public CacheGroupFilter(CacheGroup cacheGroup, CachePolicy? policy, TimeSpan? timeToLive)
        {
            _cacheGroup = cacheGroup;
            _policy = policy;
            _timeToLive = timeToLive;
        }

        public CacheGroupFilter(CacheGroup cacheGroup, CachePolicy? policy)
        {
            _cacheGroup = cacheGroup;
            _policy = policy;
        }

        public CacheGroupFilter(CacheGroup cacheGroup, TimeSpan? timeToLive)
        {
            _cacheGroup = cacheGroup;
            _timeToLive = timeToLive;
        }

        public bool Apply(ref Uri uri, ref IOptions options)
        {
            if (options.GetCacheGroup() != _cacheGroup)
                return false;

            options = options.With(_policy.Value)
                             .WithTimeToLive(_timeToLive);

            return true;
        }
    }
}