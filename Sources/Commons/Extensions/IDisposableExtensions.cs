using System;

namespace Silphid
{
    public static class IDisposableExtensions
    {
        public static T AddTo<T>(this T This, IDisposer disposer) where T : IDisposable
        {
            disposer.Add(This);
            return This;
        }
    }
}