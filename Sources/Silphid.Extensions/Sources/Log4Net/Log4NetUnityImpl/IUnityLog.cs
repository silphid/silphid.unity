using System;
using Object = UnityEngine.Object;

namespace log4net.Unity
{
    public interface IUnityLog : ILog
    {
        void Debug(string message, Object unityObject, Exception exception);
        void Debug(string message, Object unityObject);
        void Info(string message, Object unityObject, Exception exception);
        void Info(string message, Object unityObject);
        void Warn(string message, Object unityObject, Exception exception);
        void Warn(string message, Object unityObject);
        void Error(string message, Object unityObject, Exception exception);
        void Error(string message, Object unityObject);
        void Fatal(string message, Object unityObject, Exception exception);
        void Fatal(string message, Object unityObject);
    }
}