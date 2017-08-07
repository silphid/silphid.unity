using System;

namespace Silphid.Injexit
{
    public interface IContainer : IBinder, IResolver, IInjector, IDisposable
    {
        /// <summary>
        /// Creates another container sharing same logger as this container, if any.
        /// </summary>
        IContainer Create();
    }
}