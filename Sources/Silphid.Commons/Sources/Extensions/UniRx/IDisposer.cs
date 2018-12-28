using System;

namespace Silphid.Extensions
{
    public interface IDisposer
    {
        void Add(IDisposable disposable);
    }
}