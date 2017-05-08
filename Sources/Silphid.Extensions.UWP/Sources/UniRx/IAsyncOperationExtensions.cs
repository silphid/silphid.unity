using Windows.Foundation;
using Rx = UniRx;

namespace Silphid.Extensions
{
    public static class IAsyncOperationExtensions
    {
        public static Rx.IObservable<TResult> AsObservable<TResult>(this IAsyncOperation<TResult> This) =>
            Rx.Observable.FromEvent<AsyncOperationCompletedHandler<TResult>, TResult>(
                x => (asyncInfo, asyncStatus) => x(asyncInfo.GetResults()),
                x => This.Completed += x, x => This.Completed -= x);
    }
}
