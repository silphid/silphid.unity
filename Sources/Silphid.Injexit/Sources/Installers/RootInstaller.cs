using UnityEngine;

namespace Silphid.Injexit
{
    public abstract class RootInstaller : Installer
    {
        public bool LogContainer;
        public bool LogAll;

        public virtual void Start()
        {
            Logger = LogContainer ? Debug.unityLogger : null;
            Container = new Container(Logger);

            if (LogAll)
                Container.BindInstance(Debug.unityLogger);

            Logger?.Log($"Installing {GetType().Name}");

            OnBind(Container);
            InjectScene();
            OnReady();
        }
    }
}