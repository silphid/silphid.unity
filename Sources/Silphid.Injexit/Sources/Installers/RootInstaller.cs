using UnityEngine;

namespace Silphid.Injexit
{
    public abstract class RootInstaller : Installer
    {
        public bool LogContainer;
        public bool LogAll;

        public void Start()
        {            
            Container = new Container(LogContainer ? Debug.unityLogger : null);

            if (LogAll)
                Container.BindInstance(Debug.unityLogger);

            Debug.Log($"Installing {GetType().Name}");

            Install();
            InjectScene();
        }
    }
}