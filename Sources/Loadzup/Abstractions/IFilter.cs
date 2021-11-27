using System;

namespace Silphid.Loadzup
{
    public interface IFilter
    {
        /// <summary>
        /// Tries to transform given Uri and/or IOptions if they match some criteria and returns
        /// whether transformation occured.
        /// </summary>
        bool Apply(ref Uri uri, ref IOptions options);
    }
}