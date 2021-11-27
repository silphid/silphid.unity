using System;

namespace Silphid
{
    /// <summary>
    /// Object that can take the responsibility to dispose other disposables when
    /// it is itself disposed or destroyed.
    /// </summary>
    public interface IDisposer
    {
        void Add(IDisposable disposable);
    }
}