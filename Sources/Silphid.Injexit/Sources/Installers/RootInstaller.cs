using System;
using System.IO;
using System.Xml;
using log4net;
using UnityEngine;
using log4net.Config;
using Silphid.Loadzup.Http;
using Silphid.Sequencit;
using UniRx;

namespace Silphid.Injexit
{
    public abstract class RootInstaller : Installer
    {
        public string LogConfigPath = "Log4net.xml";
        public string LogConfigPathToWatchForChanges = "Assets/StreamingAssets/Log4net.xml";

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

        protected override IObservable<Unit> Init() =>
            Sequence.Create(seq =>
            {
                seq.Add(InitLog);
                seq.Add(base.Init);
            });

        protected virtual IObservable<Unit> InitLog()
        {
            Debug.Log("Initializing logging");
            return LoadLogConfig()
                .Do(x =>
                {
                    ConfigureLog(x);

                    if (Application.isEditor)
                        WatchLogConfig();
                })
                .AsSingleUnitObservable();
        }

        protected IObservable<XmlDocument> LoadLogConfig() =>
            ObservableWebRequest
                .Get(Path.Combine(Application.streamingAssetsPath, LogConfigPath))
                .Select(x =>
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(x.downloadHandler.text.Replace("${DataPath}", DataPath));
                    return doc;
                });

        protected void ConfigureLog(XmlDocument doc)
        {
            var repository = LogManager.GetRepository(GetType().Assembly);
            XmlConfigurator.Configure(repository, doc.DocumentElement);
        }

        protected void WatchLogConfig()
        {
            var fullPath = Path.Combine(Environment.CurrentDirectory, LogConfigPathToWatchForChanges);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"Log config file not found for watching for changes: {fullPath}");
                return;
            }

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
    }
}