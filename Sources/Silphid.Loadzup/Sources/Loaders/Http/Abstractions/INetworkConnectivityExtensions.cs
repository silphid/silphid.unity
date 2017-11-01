using System;
using UniRx;

namespace Silphid.Loadzup.Http
{
    public static class INetworkConnectivityExtensions
    {
        public static IObservable<bool> IsOnlineObservable(this INetworkConnectivity This) =>
            This.Status
                .Select(x => x == NetworkStatus.Online)
                .DistinctUntilChanged();

        public static bool IsOnline(this INetworkConnectivity This) =>
            This.Status.Value == NetworkStatus.Online;
    }
}