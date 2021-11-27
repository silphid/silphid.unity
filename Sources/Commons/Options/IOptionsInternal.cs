using System.Collections.Generic;

namespace Silphid.Options
{
    public interface IOptionsInternal : IOptionsBase
    {
        bool HasValue(object key);
        object GetValue(object key);
        IEnumerable<object> GetValues(object key);
        IEnumerable<object> Keys { get; }
    }
}