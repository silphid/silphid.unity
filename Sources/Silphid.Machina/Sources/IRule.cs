using System;
using Silphid.Requests;

namespace Silphid.Machina
{
    public interface IRule
    {
        void Handle<TRequest>(Func<TRequest, IRequest> handler);
        void Handle<TRequest>(Action<TRequest> handler);
    }
}