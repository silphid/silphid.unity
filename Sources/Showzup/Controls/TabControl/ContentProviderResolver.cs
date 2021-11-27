using System;

namespace Silphid.Showzup
{
    public class ContentProviderResolver: IContentResolver
    {
        public IObservable<object> GetContent(object input)
        {
            return (input as IContentProvider)?.GetContent();
        }
    }
}