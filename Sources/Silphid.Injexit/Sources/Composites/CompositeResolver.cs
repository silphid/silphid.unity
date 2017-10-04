using System;
using System.Linq;
using log4net;
using Silphid.Extensions;

namespace Silphid.Injexit
{
    public class CompositeResolver : IResolver
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CompositeResolver));
        
        private readonly IResolver[] _resolvers;

        public CompositeResolver(params IResolver[] resolvers)
        {
            if (resolvers == null)
                throw new ArgumentNullException(nameof(resolvers));

            if (resolvers.Length == 0)
                throw new ArgumentException($"Argument {nameof(resolvers)} must contain at least one resolver.");
            
            _resolvers = resolvers
                .WhereNotNull()
                .ToArray();
        }

        public IContainer Create() =>
            _resolvers.First().Create();

        public Func<IResolver, object> ResolveFactory(Type abstractionType, string name = null)
        {
            if (_resolvers.Length == 0)
                throw new InvalidOperationException("CompositeResolver must contain at least one child resolver.");
         
            Log.Debug($"Resolving dependency '{name}' of type {abstractionType.Name}");
            
            UnresolvedTypeException exception = null;
        
            foreach (var resolver in _resolvers)
            {
                try
                {
                    return resolver.ResolveFactory(abstractionType, name);
                }
                catch (UnresolvedTypeException ex)
                {
                    exception = ex;
                }
            }
        
            // ReSharper disable once PossibleNullReferenceException
            throw exception;
        }
    }
}