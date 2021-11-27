using Silphid.DataTypes;

namespace Silphid.Loadzup
{
    public class CacheGroup : TypeSafeEnum<CacheGroup>
    {
        /// <summary>
        /// Images
        /// </summary>
        public static readonly CacheGroup Image = new CacheGroup(nameof(Image));

        /// <summary>
        /// Application configuration
        /// </summary>
        public static readonly CacheGroup AppConfig = new CacheGroup(nameof(AppConfig));

        /// <summary>
        /// Localized text
        /// </summary>
        public static readonly CacheGroup CopyDeck = new CacheGroup(nameof(CopyDeck));

        /// <summary>
        /// Data that rarely changes and could potentially be cached for a long time
        /// </summary>
        public static readonly CacheGroup Stable = new CacheGroup(nameof(Stable));

        /// <summary>
        /// Data that changes regularly but could be cached momentarily (ie: duration of session)
        /// </summary>
        public static readonly CacheGroup Transient = new CacheGroup(nameof(Transient));

        /// <summary>
        /// Data that changes constantly and should never be cached.
        /// </summary>
        public static readonly CacheGroup Volatile = new CacheGroup(nameof(Volatile));

        public CacheGroup(string name)
            : base(name) {}
    }
}