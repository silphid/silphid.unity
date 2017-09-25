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
            Log.Debug($"Installing {GetType().Name}");

            Container = new Container(new Reflector());
            
            Log.Debug("Configuring bindings...");
            OnBind();

            Log.Debug("Injecting scene dependencies...");
            InjectScene();
            
            Log.Debug("Launching app...");
            OnReady();
        }

        protected virtual void OnConfigureLogging()
        {
            var textAsset = Resources.Load<TextAsset>(LogResourceFile); 
            var xmldoc = new XmlDocument();
            xmldoc.LoadXml (textAsset.text);
            
            XmlConfigurator.Configure(xmldoc.DocumentElement);
        }
    }
}