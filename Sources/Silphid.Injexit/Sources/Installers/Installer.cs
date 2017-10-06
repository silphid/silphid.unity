using log4net;
using UnityEngine;

namespace Silphid.Injexit
{
    public abstract class Installer : MonoBehaviour, IInstaller
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Installer));
        
        public IContainer Container { get; protected set; }
        protected abstract void OnBind();
        protected virtual void OnReady() {}

        protected void InjectScene()
        {
            Log.Info($"Injecting dependencies into {gameObject.scene.name} scene...");

            Container.InjectScene(gameObject.scene);
        }

        private void OnDestroy()
        {
            Container?.Dispose();
        }
    }
}