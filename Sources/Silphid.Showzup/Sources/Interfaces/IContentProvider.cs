using System;

namespace Silphid.Showzup
{
    public interface IContentProvider
    {
        IObservable<object> GetContent();
    }
}