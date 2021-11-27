using System;
using System.Linq;

namespace Silphid.Showzup
{
    public class CompositeContentResolver: IContentResolver
    {
        private readonly IContentResolver[] _resolvers;

        public CompositeContentResolver(IContentResolver[] resolvers)
        {
            _resolvers = resolvers;
        }

        public IObservable<object> GetContent(object input)
        {
            return _resolvers.Select(x => x.GetContent(input)).FirstOrDefault(x => x != null);
        }
    }
}