using System.Xml;
using log4net;
using UnityEngine;
using log4net.Config;

namespace Silphid.Injexit
{
    public abstract class RootInstaller : Installer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RootInstaller));
        
        public string LogResourceFile = "Log4net";
        
        public virtual void Start()
        {
            OnConfigureLogging();
            Log.Info($"Installing {GetType().Name}...");

            Container = new Container(new Reflector());
            
            Log.Info("Configuring bindings...");
            OnBind();

            InjectScene();
            
            Log.Info($"Completing {GetType().Name}...");
            OnReady();
            
            Log.Info($"Completed {GetType().Name}.");
        }

        protected virtual void OnConfigureLogging()
        {
            var textAsset = Resources.Load<TextAsset>(LogResourceFile);
            var text = textAsset.text.Replace("${DataPath}", Application.dataPath);
            var xmldoc = new XmlDocument();
            xmldoc.LoadXml (text);

            var repository = LogManager.GetRepository(GetType().Assembly);
            XmlConfigurator.Configure(repository, xmldoc.DocumentElement);
        }
    }
}