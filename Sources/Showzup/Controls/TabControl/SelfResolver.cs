using System;
using UniRx;

namespace Silphid.Showzup
{
    public class SelfResolver: IContentResolver
    {
        public IObservable<object> GetContent(object input)
        {
            return Observable.Return(input);
        }
    }
}