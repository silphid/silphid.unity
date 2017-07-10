using UnityEngine;

namespace Silphid.Injexit
{
    public abstract class Installer : MonoBehaviour, IInstaller
    {
        public IContainer Container { get; protected set; }
        protected abstract void Install();

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