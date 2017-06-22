using System;
using System.Linq;
using Silphid.Extensions;

namespace Silphid.Showzup.Injection
{
    public class CompositeResolver : IResolver
    {
        private readonly IResolver[] _resolvers;

        public CompositeResolver(params IResolver[] resolvers)
        {
            _resolvers = resolvers;
        }

        public object Resolve(Type abstractionType, IResolver subResolver = null, bool isOptional = false) =>
            _resolvers
                .WhereNotNull()
                .Select(x => x.Resolve(abstractionType, subResolver, isOptional))
                .FirstNotNullOrDefault();
    }
}