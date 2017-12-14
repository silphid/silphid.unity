using System;
using System.IO;
using System.Xml;
using log4net;
using UnityEngine;
using log4net.Config;
using UniRx;

namespace Silphid.Injexit
{
    public class LogConfigurator
    {
        private readonly string _dataPath;
        private readonly string _resourcePath;
        private readonly string _fileName;
        
        private static string DataPath =>
            #if UNITY_EDITOR
            Path.Combine(Application.dataPath, "../AppData");
            #else
            Application.persistentDataPath;
            #endif

        private string ConfigDataPath =>
            Path.Combine(DataPath, $"{_dataPath}{_fileName}.xml");

        private string ConfigResourcePath =>
            Path.Combine(Environment.CurrentDirectory, $"{_resourcePath}{_fileName}.xml");

        public LogConfigurator(string dataPath = "", string resourcePath = "Assets/Resources/", string fileName = "Log4net")
        {
            _dataPath = dataPath;
            _resourcePath = resourcePath;
            _fileName = fileName;
        }

        public void Configure()
        {
            Load();

            #if UNITY_EDITOR
            Watch();
            #endif
        }

        #if UNITY_EDITOR
        
        private void Watch()
        {
            var path = GetPathToWatch();
            if (path == null)
            {
                Debug.LogWarning("Log config file not found for watching changes, " +
                                 $"neither in ConfigDataPath: {ConfigDataPath} " +
                                 $"nor in ConfigResourcePath: {ConfigResourcePath}");
                return;
            }
            
            Debug.Log($"Watching log config file for changes: {path}");

            Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Select(_ => File.GetLastWriteTime(path))
                .DistinctUntilChanged()
                .Skip(1)
                .Subscribe(_ =>
                {
                    Debug.Log("Reloading changes made to log config file");
                    LoadFromResources();
                });
        }

        private string GetPathToWatch()
        {
            if (File.Exists(ConfigDataPath))
                return ConfigDataPath;

            if (File.Exists(ConfigResourcePath))
                return ConfigResourcePath;

            return null;
        }
        
        #endif

        private void Load()
        {
            var text = LoadFromDataPath() ??
                       LoadFromResources();
            
            Debug.Log($"Injecting log config with DataPath: {DataPath}");
            text = text.Replace("${DataPath}", DataPath);
            var xmldoc = new XmlDocument();
            xmldoc.LoadXml(text);

            var repository = LogManager.GetRepository(GetType().Assembly);
            XmlConfigurator.Configure(repository, xmldoc.DocumentElement);
        }

        private string LoadFromDataPath()
        {
            var path = ConfigDataPath;
            if (File.Exists(path))
            {
                Debug.Log($"Loading log config from DataPath: {ConfigDataPath}");
                return File.ReadAllText(path);
            }
            return null;
        }

        private string LoadFromResources()
        {
            Debug.Log("Loading log config from resources");
            return Resources.Load<TextAsset>(_fileName).text;
        }
    }
}