using System;
using System.Linq;
using Silphid.Extensions;

namespace Silphid.Injexit
{
    public class CompositeResolver : IResolver
    {
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

        public Func<IResolver, object> ResolveFactory(Type abstractionType, string id = null, bool isOptional = false) =>
            _resolvers
                .Select((index, x) => x.ResolveFactory(abstractionType, id, true))
                .FirstNotNullOrDefault()
            ?? ThrowIfNotOptional(abstractionType, isOptional);
        
        private Func<IResolver, object> ThrowIfNotOptional(Type abstractionType, bool isOptional)
        {
            if (!isOptional)
                throw new Exception($"No mapping for required type {abstractionType.Name}.");

            return null;
        }
    }
}