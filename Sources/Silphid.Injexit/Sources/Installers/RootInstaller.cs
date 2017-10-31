using System;
using System.IO;
using System.Xml;
using log4net;
using UnityEngine;
using log4net.Config;
using UniRx;

namespace Silphid.Injexit
{
    public abstract class RootInstaller : Installer
    {
        public string LogResourceFile = "Log4net";
        public string LogPathToWatchForChanges = "Assets/Resources/Log4net.xml";

        private static string DataPath =>
            Application.isEditor
                ? Path.Combine(Application.dataPath, "../AppData")
                : Application.persistentDataPath;

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
            Debug.Log("Configuring logging");
            ConfigureLogging();
            
            base.Start();
        }

        protected virtual void ConfigureLogging()
        {
            LoadLogConfig();

            if (Application.isEditor)
            {
                var fullPath = Path.Combine(Environment.CurrentDirectory, LogPathToWatchForChanges);
                if (File.Exists(fullPath))
                {
                    Debug.Log($"Watching log config file for changes: {fullPath}");

                    Observable
                        .Interval(TimeSpan.FromSeconds(1))
                        .Select(_ => File.GetLastWriteTime(fullPath))
                        .DistinctUntilChanged()
                        .Skip(1)
                        .Subscribe(_ =>
                        {
                            Debug.Log("Reloading changes made to log config file");
                            LoadLogConfig();
                        });
                }
                else
                    Debug.LogWarning($"Log config file not found for watching for changes: {fullPath}");
            }
        }

        private void LoadLogConfig()
        {
            var textAsset = Resources.Load<TextAsset>(LogResourceFile);
            var text = textAsset.text.Replace("${DataPath}", DataPath);
            var xmldoc = new XmlDocument();
            xmldoc.LoadXml(text);

            var repository = LogManager.GetRepository(GetType().Assembly);
            XmlConfigurator.Configure(repository, xmldoc.DocumentElement);
        }
    }
}