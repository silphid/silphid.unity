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
            _resolvers = resolvers
                .WhereNotNull()
                .ToArray();
        }

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