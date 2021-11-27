using System;

namespace Silphid.Injexit
{
    public interface IContainer : IBinder, IResolver, IInjector, IDisposable
    {
        /// <summary>
        /// Forces all bindings marked with AsEagerSingle() to be resolved/instantiated.
        /// If you add more AsEagerSingle() bindings after calling this method, you
        /// may call the method again to force the instantiation of those new bindings. 
        /// </summary>
        void InstantiateEagerSingles(IResolver resolver);
    }
}