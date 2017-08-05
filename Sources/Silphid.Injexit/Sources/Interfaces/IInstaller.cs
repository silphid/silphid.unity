using UnityEngine;

namespace Silphid.Injexit
{
    public interface IInstaller
    {
        /// <summary>
        /// Container that is configured by this installer, used for nesting containers.
        /// </summary>
        IContainer Container { get; }
        
        ILogger Logger { get; }
    }
}