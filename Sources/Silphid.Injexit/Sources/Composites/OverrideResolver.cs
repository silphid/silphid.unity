using System;
using log4net;

namespace Silphid.Injexit
{
    public class OverrideResolver : IResolver
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OverrideResolver));

        private readonly IResolver _baseResolver;
        private readonly IResolver _overrideResolver;
        private readonly bool _isRecursive;

        public OverrideResolver(IResolver baseResolver, IResolver overrideResolver, bool isRecursive)
        {
            if (overrideResolver == null)
                throw new ArgumentNullException(nameof(overrideResolver));
            
            if (baseResolver == null)
                throw new ArgumentNullException(nameof(baseResolver));

            _baseResolver = baseResolver;
            _overrideResolver = overrideResolver;
            _isRecursive = isRecursive;
        }

        public IContainer Create() =>
            _overrideResolver.Create();

        public Func<IResolver, object> ResolveFactory(Type abstractionType, string name = null)
        {
            Log.Debug($"Resolving dependency '{name}' of type {abstractionType.Name}");
            
            try
            {
                return _overrideResolver.ResolveFactory(abstractionType, name);
            }
            catch (UnresolvedTypeException)
            {
                return _baseResolver.ResolveFactory(abstractionType, name);
            }
        }

        public IResolver BaseResolver =>
            _isRecursive
            ? this
            : _baseResolver.BaseResolver;
    }
}