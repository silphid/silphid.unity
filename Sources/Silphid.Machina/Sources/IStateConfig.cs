using System;
using Silphid.Requests;

namespace Silphid.Machina
{
    public interface IStateConfig
    {
        void Handle<T>(Func<T, IRequest> handler);
        void Handle<T>(Action<T> handler);
    }
}