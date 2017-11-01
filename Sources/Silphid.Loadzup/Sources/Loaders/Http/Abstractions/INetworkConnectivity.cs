using UniRx;

namespace Silphid.Loadzup.Http
{
    public interface INetworkConnectivity
    {
        IReadOnlyReactiveProperty<NetworkStatus> Status { get; }
    }
}