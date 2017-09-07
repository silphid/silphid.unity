using System;

namespace Silphid.Machina
{
    public interface IStateConfig
    {
        void Handle<T>(Func<T, bool> handler);
        void Handle<T>(Action<T> handler);
    }
}