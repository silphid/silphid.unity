using System;

namespace Silphid.Machina.Internals
{
    public interface IStateConfig
    {
        void On<T>(Func<T, bool> handler);
        void On<T>(Action<T> handler);
    }
}