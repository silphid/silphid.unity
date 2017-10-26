using System.IO;
using System.Xml;
using log4net;
using UnityEngine;
using log4net.Config;

namespace Silphid.Injexit
{
    public abstract class RootInstaller : Installer
    {
        public string LogResourceFile = "Log4net";

        private static string DataPath =>
            Application.isEditor
                ? Path.Combine(Application.dataPath, "../AppData")
                : Application.dataPath;

        protected override IContainer CreateContainer() =>
            new Container(new Reflector(), ShouldInject);

        protected virtual bool ShouldInject(object obj)
        {
            var ns = obj.GetType().Namespace;
            return ns == null ||
                   !ns.StartsWith("Unity") &&
                   !ns.StartsWith("TMPro");
        }

        public override void Start()
        {
            Debug.Log("Configuring logging...");
            ConfigureLogging();
            
            base.Start();
        }

        protected virtual void ConfigureLogging()
        {
            var textAsset = Resources.Load<TextAsset>(LogResourceFile);
            var text = textAsset.text.Replace("${DataPath}", DataPath);
            var xmldoc = new XmlDocument();
            xmldoc.LoadXml (text);

            var repository = LogManager.GetRepository(GetType().Assembly);
            XmlConfigurator.Configure(repository, xmldoc.DocumentElement);
        }
    }
}