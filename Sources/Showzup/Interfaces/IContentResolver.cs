using System;

namespace Silphid.Showzup
{
    public interface IContentResolver
    {
        IObservable<object> GetContent(object input);
    }
}