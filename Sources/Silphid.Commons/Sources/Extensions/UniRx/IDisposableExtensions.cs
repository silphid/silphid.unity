using System;

namespace Silphid.Extensions
{
    public static class IDisposableExtensions
    {
        public static IDisposable AddTo(this IDisposable This, IDisposer disposer)
        {
            disposer.Add(This);
            return This;
        }
    }
}