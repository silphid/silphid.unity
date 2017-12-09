using System;
using log4net.Appender;
using log4net.Core;
using Object = UnityEngine.Object;

namespace log4net.Unity
{
    public class UnityLogWrapper : LogImpl, IUnityLog
    {
        public UnityLogWrapper(ILogger log)
            : base(log)
        {
        }

        public void Debug(string message, Object unityObject, Exception exception)
        {
            Debug(new UnityObjectPair(message, unityObject), exception);
        }
        public void Debug(string message, Object unityObject)
        {
            Debug(new UnityObjectPair(message, unityObject));
        }
        public void Info(string message, Object unityObject, Exception exception)
        {
            Info(new UnityObjectPair(message, unityObject), exception);
        }
        public void Info(string message, Object unityObject)
        {
            Info(new UnityObjectPair(message, unityObject));
        }
        public void Warn(string message, Object unityObject, Exception exception)
        {
            Warn(new UnityObjectPair(message, unityObject), exception);
        }
        public void Warn(string message, Object unityObject)
        {
            Warn(new UnityObjectPair(message, unityObject));
        }
        public void Error(string message, Object unityObject, Exception exception)
        {
            Error(new UnityObjectPair(message, unityObject), exception);
        }
        public void Error(string message, Object unityObject)
        {
            Error(new UnityObjectPair(message, unityObject));
        }
        public void Fatal(string message, Object unityObject, Exception exception)
        {
            Fatal(new UnityObjectPair(message, unityObject), exception);
        }
        public void Fatal(string message, Object unityObject)
        {
            Fatal(new UnityObjectPair(message, unityObject));
        }
    }
}