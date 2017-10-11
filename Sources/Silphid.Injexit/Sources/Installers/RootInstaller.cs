using System.Xml;
using log4net;
using UnityEngine;
using log4net.Config;

namespace Silphid.Injexit
{
    public abstract class RootInstaller : Installer
    {
        public string LogResourceFile = "Log4net";

        protected override IContainer CreateContainer() =>
            new Container(new Reflector());

        public override void Start()
        {
            Debug.Log("Configuring logging...");
            ConfigureLogging();
            
            base.Start();
        }

        protected virtual void ConfigureLogging()
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