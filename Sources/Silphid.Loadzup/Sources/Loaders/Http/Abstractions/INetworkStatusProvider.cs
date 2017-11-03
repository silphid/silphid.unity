using UniRx;

namespace Silphid.Loadzup.Http
{
    public interface INetworkStatusProvider
    {
        IReadOnlyReactiveProperty<NetworkStatus> Status { get; }
    }
}