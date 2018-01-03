using System;

namespace Silphid.Machina
{
    public interface IRule
    {
        void Handle<TRequest>(Func<TRequest, bool> handler);
        void Handle<TRequest>(Action<TRequest> handler);
        void HandlePartially<TRequest>(Action<TRequest> handler);
    }
}