using System;

namespace Silphid.Injexit
{
    public interface IContainer : IBinder, IResolver, IInjector, IDisposable
    {
    }
}