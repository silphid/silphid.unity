using JetBrains.Annotations;
using log4net.Appender;
using log4net.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace log4net.Unity
{
    [UsedImplicitly]
    public class UnityLogAppender : AppenderSkeleton
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            UnityObjectPair unityObjectPair = loggingEvent.MessageObject as UnityObjectPair;
            Object unityObject = (null == unityObjectPair) ? null : unityObjectPair.UnityObject;
            string renderLoggingEvent = loggingEvent.RenderedMessage;
            if (Layout != null)
            {
                renderLoggingEvent = RenderLoggingEvent(loggingEvent);
            }
            Level level = loggingEvent.Level;
            if (level < Level.Warn)
            {
                Debug.Log(renderLoggingEvent, unityObject);
            }
            else if (level < Level.Error)
            {
                Debug.LogWarning(renderLoggingEvent, unityObject);
            }
            else if (level < Level.Off)
            {
                Debug.LogError(renderLoggingEvent, unityObject);
            }
            if (null != loggingEvent.ExceptionObject)
            {
                Debug.LogException(loggingEvent.ExceptionObject, unityObject);
            }
        }
    }
}
