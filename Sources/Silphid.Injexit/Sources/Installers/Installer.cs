using log4net;
using UnityEngine;

namespace Silphid.Injexit
{
    public abstract class Installer : MonoBehaviour, IInstaller
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Installer));
        
        public IContainer Container { get; protected set; }

        protected abstract IContainer CreateContainer();
        protected abstract void Configure();
        protected virtual void Complete() {}

        public virtual void Start()
        {
            Log.Info($"Creating container for {GetType().Name}...");
            Container = CreateContainer();
            
            Log.Info("Configuring bindings...");
            Configure();

            Log.Info("Instantiating eager singles...");
            Container.InstantiateEagerSingles();

            Log.Info($"Injecting dependencies into {gameObject.scene.name} scene...");
            InjectScene();

            Log.Info($"Completing {GetType().Name}...");
            Complete();
            
            Log.Info($"Completed {GetType().Name}.");
        }

        protected virtual void InjectScene()
        {
            Container.InjectScene(gameObject.scene);
        }

        private void OnDestroy()
        {
            Container?.Dispose();
        }
    }
}