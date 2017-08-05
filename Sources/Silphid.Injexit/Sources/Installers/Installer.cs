using UnityEngine;

namespace Silphid.Injexit
{
    public abstract class Installer : MonoBehaviour, IInstaller
    {
        public IContainer Container { get; protected set; }
        public ILogger Logger { get; protected set; }
        protected abstract void OnBind(IBinder binder);
        protected virtual void OnReady() {}

        protected void InjectScene()
        {
            Container.InjectScene(gameObject.scene);
        }

        private void OnDestroy()
        {
            Container.Dispose();
        }
    }
}